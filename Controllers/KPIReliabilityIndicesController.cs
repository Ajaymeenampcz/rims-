using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPIReliabilityIndicesController : ApiController
    {
        [HttpGet]
        [Route("api/KPIReliabilityIndices/{company}/{circle}/{metric}/{quarter1}/{year1}/{quarter2}/{year2}")]
        public IHttpActionResult GetKpiData(
            string company,
            string circle,
            string metric,
            string quarter1,
            string year1,
            string quarter2,
            string year2)
        {
            try
            {
                // Validate required parameters
                if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(circle) || string.IsNullOrEmpty(metric))
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Kindly enter valid Company, Circle, and Metric values." });
                }

                // Ensure metric is valid
                var validMetrics = new HashSet<string> { "saifi", "saidi", "reliability" };
                if (!validMetrics.Contains(metric.ToLower()))
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Invalid Metric value provided. Choose either 'saifi', 'saidi', or 'reliability'." });
                }

                // Define valid circles for validation
                var validCircles = new HashSet<string>
                {
                    "Bhopal", "Gwalior", "Central Discom", "Rewa",
                    "Sagar", "Shahdol", "Jabalpur", "EAST Discom",
                    "Indore", "Ujjain", "West Discom"
                };

                if (!validCircles.Contains(circle))
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Invalid Circle value provided." });
                }

                List<dynamic> kpiDataList = new List<dynamic>();

                string connectionString = ConfigurationManager.ConnectionStrings["constr2"].ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    Dictionary<string, string> quarterYearPairs = new Dictionary<string, string>();

                    void AddQuarterYear(string quarter, string year)
                    {
                        if (!string.IsNullOrEmpty(quarter) && !string.IsNullOrEmpty(year))
                        {
                            if (!quarterYearPairs.ContainsKey(quarter))
                            {
                                quarterYearPairs.Add(quarter, year);
                            }
                        }
                    }

                    AddQuarterYear(quarter1, year1);
                    AddQuarterYear(quarter2, year2);

                    if (quarterYearPairs.Count == 0)
                    {
                        return Content(HttpStatusCode.BadRequest, new { error = "Please provide at least one valid Quarter and Year combination." });
                    }

                    connection.Open();

                    foreach (var pair in quarterYearPairs)
                    {
                        string query = @"
                            SELECT 
                                circle, 
                                saifi = CASE WHEN @metric = 'saifi' THEN saifi ELSE NULL END,
                                saidi = CASE WHEN @metric = 'saidi' THEN saidi ELSE NULL END,
                                reliability_index_of_11_kv = CASE WHEN @metric = 'reliability' THEN reliability_index_of_11_kv ELSE NULL END
                            FROM 
                                discom_d21_saifisaidi
                            WHERE 
                                company_name = @company
                                AND quarter_report = @quarter
                                AND year_report = @year
                                AND (
                                    (@circle = 'Bhopal' AND circle IN ('Bhopal Region','BR'))
                                    OR (@circle = 'Gwalior' AND circle IN ('Gwalior Region','GR'))
                                    OR (@circle = 'Central Discom' AND circle IN ('CZ','Discom Region','Grand Total'))
                                    OR (@circle = 'Rewa' AND circle IN ('zRewa_Region_Total'))
                                    OR (@circle = 'Sagar' AND circle IN ('zSagar_Region_Total'))
                                    OR (@circle = 'Shahdol' AND circle IN ('zShahdol_Region_Total'))
                                    OR (@circle = 'Jabalpur' AND circle IN ('zJabalpur_Region_Total'))
                                    OR (@circle = 'EAST Discom' AND circle IN ('zEZ'))
                                    OR (@circle = 'Indore' AND circle IN ('IR TOTAL'))
                                    OR (@circle = 'Ujjain' AND circle IN ('UR TOTAL'))
                                    OR (@circle = 'West Discom' AND circle IN ('WZTOTAL'))
                                    OR (circle = @circle)
                                );";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@company", company);
                            command.Parameters.AddWithValue("@circle", circle);
                            command.Parameters.AddWithValue("@quarter", pair.Key);
                            command.Parameters.AddWithValue("@year", pair.Value);
                            command.Parameters.AddWithValue("@metric", metric.ToLower());

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    dynamic kpiData = new System.Dynamic.ExpandoObject();
                                    kpiData.Company = company;
                                    kpiData.Region = reader["circle"].ToString();
                                    kpiData.Quarter = pair.Key;
                                    kpiData.Year = pair.Value;

                                    if (metric.ToLower() == "saifi")
                                        kpiData.saifi = reader["saifi"]?.ToString();
                                    else if (metric.ToLower() == "saidi")
                                        kpiData.saidi = reader["saidi"]?.ToString();
                                    else if (metric.ToLower() == "reliability")
                                        kpiData.reliability_index_of_11_kv = reader["reliability_index_of_11_kv"]?.ToString();

                                    kpiDataList.Add(kpiData);
                                }
                            }
                        }
                    }

                    connection.Close();
                }

                if (kpiDataList.Count == 0)
                {
                    return Content(HttpStatusCode.NotFound, new { error = "No KPI data found for the specified parameters." });
                }

                return Ok(kpiDataList);
            }
            catch (SqlException ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { error = "An SQL-related error occurred while processing the request.", message = ex.Message });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { error = "An internal server error occurred: " + ex.Message });
            }
        }
    }
}
