using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPIdtr14failureController : ApiController
    {
        [HttpGet]
        [Route("api/KPIdtr14failure/{company}/{circle}/{quarter1}/{year1}/{quarter2}/{year2}/{quarter3}/{year3}/{quarter4}/{year4}")]
        public IHttpActionResult GetKpiData(
            string company,
            string circle,
            string quarter1,
            string year1,
            string quarter2,
            string year2,
            string quarter3,
            string year3,
            string quarter4,
            string year4)
        {
            try
            {
                // Validate required parameters
                if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(circle))
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Kindly enter valid Company and Circle values." });
                }

                // Define valid circles for validation
                var validCircles = new HashSet<string>
                {
                    "Bhopal", "Gwalior", "Central Discom", "Rewa",
                    "Sagar", "Shahdol", "Jabalpur", "EAST Discom",
                    "Indore", "Ujjain", "West Discom"
                };

                // Check if the circle value is valid
                if (!validCircles.Contains(circle))
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Invalid Circle value provided." });
                }

                // List to store KPI data
                List<KPIdtr14failureData> kpiDataList = new List<KPIdtr14failureData>();

                // Connection string
                string connectionString = ConfigurationManager.ConnectionStrings["constr2"].ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Collect all valid quarter-year pairs into a dictionary
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

                    // Add valid quarter-year combinations
                    AddQuarterYear(quarter1, year1);
                    AddQuarterYear(quarter2, year2);
                    AddQuarterYear(quarter3, year3);
                    AddQuarterYear(quarter4, year4);

                    // Check if at least one quarter-year combination was added
                    if (quarterYearPairs.Count == 0)
                    {
                        return Content(HttpStatusCode.BadRequest, new { error = "Please provide at least one valid Quarter and Year combination." });
                    }

                    // Open connection once before iterating
                    connection.Open();

                    // Iterate through all provided quarter-year combinations and fetch data
                    foreach (var pair in quarterYearPairs)
                    {
                        string query = @"
                            SELECT 
                                no_of_dtr_beg_qtr,
                                fail_dtr_beg_of_qtr_no_of,
                                fail_dtr_beg_of_qtr_per,
                                dtr_def_end_qtr_no_of,
                                dtr_def_end_qtr_per,
                                per_dtr_rplcd_in_time_line,
                                circle -- Ensure circle is selected
                            FROM 
                                discom_d14_dtrfailure
                            WHERE 
                                company_name = @company
                                AND quarter_report = @quarter
                                AND year_report = @year
                                AND (
                                    (@circle = 'Bhopal' AND circle IN ('Bhopal Region'))
                                    OR (@circle = 'Gwalior' AND circle IN ('Gwalior Region'))
                                    OR (@circle = 'Central Discom' AND circle IN ('CZ Total'))
                                    OR (@circle = 'Rewa' AND circle IN ('Rewa Region'))
                                    OR (@circle = 'Sagar' AND circle IN ('Sagar_Region'))
                                    OR (@circle = 'Shahdol' AND circle IN ('Shahdol Region'))
                                    OR (@circle = 'Jabalpur' AND circle IN ('Jabalpur Region'))
                                    OR (@circle = 'EAST Discom' AND circle IN ('EZ'))
                                    OR (@circle = 'Indore' AND circle IN ('Indore Region TOTAL'))
                                    OR (@circle = 'Ujjain' AND circle IN ('Ujjain Region TOTAL'))
                                    OR (@circle = 'West Discom' AND circle IN ('WZTOTAL'))
                                    OR (circle = @circle)  -- Fallback for any other specific circle
                                );";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Add parameters
                            command.Parameters.AddWithValue("@company", company);
                            command.Parameters.AddWithValue("@circle", circle);
                            command.Parameters.AddWithValue("@quarter", pair.Key); // Quarter
                            command.Parameters.AddWithValue("@year", pair.Value); // Year

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    KPIdtr14failureData kpiData = new KPIdtr14failureData
                                    {
                                        Company = company,
                                        Region = reader["circle"].ToString(), // Correctly retrieving circle value
                                        Quarter = pair.Key,
                                        Year = pair.Value,
                                        TransformersBeginQuarter = reader["no_of_dtr_beg_qtr"].ToString(),
                                        DefectiveTransformersStart = reader["fail_dtr_beg_of_qtr_no_of"]?.ToString(),
                                        DefectiveTransformersStartPercent = reader["fail_dtr_beg_of_qtr_per"].ToString(),
                                        TotalDefectiveTransformersEnd = reader["dtr_def_end_qtr_no_of"].ToString(),
                                        DefectiveTransformersEndPercent = reader["dtr_def_end_qtr_per"]?.ToString(),
                                        TransformersReplacedInTimePercent = reader["per_dtr_rplcd_in_time_line"]?.ToString(),
                                    };

                                    kpiDataList.Add(kpiData);
                                }
                            }
                        }
                    }

                    connection.Close(); // Close the connection after iteration
                }

                // If no data is found
                if (kpiDataList.Count == 0)
                {
                    return Content(HttpStatusCode.NotFound, new { error = "No KPI data found for the specified parameters." });
                }

                // Return the KPI data as JSON
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

    // Model for KPI data
    public class KPIdtr14failureData
    {
        public string Company { get; set; }
        public string Region { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string TransformersBeginQuarter { get; set; }
        public string DefectiveTransformersStart { get; set; }
        public string DefectiveTransformersStartPercent {  get; set; }
        public string TotalDefectiveTransformersEnd { get; set; }
        public string DefectiveTransformersEndPercent { get; set; }
        public string TransformersReplacedInTimePercent { get; set; }
    }
}
