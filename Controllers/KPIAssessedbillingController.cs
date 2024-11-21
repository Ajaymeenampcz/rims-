using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPIAssessedbillingController : ApiController
    {
        [HttpGet]
        [Route("api/KPIAssessedbilling/{company}/{circle}/{dtr_type}/{quarter1}/{year1}/{quarter2}/{year2}/{quarter3}/{year3}/{quarter4}/{year4}")]
        public IHttpActionResult GetKpiData(string company, string circle, string dtr_type, string quarter1, string year1, string quarter2, string year2, string quarter3, string year3, string quarter4, string year4)
        {
            try
            {
                // Validate required parameters
                if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(circle) || string.IsNullOrEmpty(dtr_type))
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Kindly enter valid Company, Region, and Report Type values." });
                }

                // List to store KPI data
                List<KpiDataDTRMETERING> kpiDataList = new List<KpiDataDTRMETERING>();

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
                            circle,
                            dtr_type,
                            total_dtr, 
                            dtr_inst_with_mtr, 
                            dtr_with_wrkng_mtr,
                            ROUND((CAST(dtr_inst_with_mtr AS FLOAT) / CAST(total_dtr AS FLOAT)) * 100, 2) AS per_dtr_mtr,
                            per_dtr_wrkng_mtr
                        FROM 
                            discom_d13_dtrmetering
                        WHERE 
                            company_name = @company
                            AND dtr_type = @dtr_type
                            AND quarter_report = @quarter
                            AND year_report = @year
                            AND (
                                (@circle = 'Bhopal' AND circle IN ('Total Bhopal Region', 'Bhopal Region')) OR 
                                (@circle = 'Gwalior' AND circle IN ('Total Gwalior Region', 'Gwalior Region')) OR 
                                (@circle = 'Central Discom' AND circle IN ('Total Central Discom', 'Central Discom')) OR 
                                (@circle = 'Rewa' AND circle IN ('Rewa Region', 'Rewa_Total', 'Rewa_total')) OR 
                                (@circle = 'Sagar' AND circle ='Sagar_total') OR 
                                (@circle = 'Shahdol' AND circle IN ('Shahdol_total', 'Shahdol Region', 'Shahdol_Total')) OR 
                                (@circle = 'Jabalpur' AND circle IN ('Jabalpur Region', 'Jabalpur_Total')) OR 
                                (@circle = 'EAST Discom' AND circle IN ('Total_EZ', 'EZ')) OR 
                                (@circle = 'Indore' AND circle IN ('Indore Region TOTAL')) OR 
                                (@circle = 'Ujjain' AND circle IN ('Ujjain Region TOTAL')) OR 
                                (@circle = 'West Discom' AND circle IN ('WZTOTAL'))  
                              
                            )";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Add parameters
                            command.Parameters.AddWithValue("@company", company);
                            command.Parameters.AddWithValue("@dtr_type", dtr_type);
                            command.Parameters.AddWithValue("@circle", circle);
                            command.Parameters.AddWithValue("@quarter", pair.Key); // Quarter
                            command.Parameters.AddWithValue("@year", pair.Value); // Year

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    KpiDataDTRMETERING kpiData = new KpiDataDTRMETERING
                                    {
                                        Company = company,
                                        Region = reader["circle"].ToString(),
                                        Quarter = pair.Key,
                                        Year = pair.Value,
                                        dtr_type = reader["dtr_type"].ToString(),
                                        total_dtr = reader["total_dtr"]?.ToString(),
                                        dtr_inst_with_mtr = reader["dtr_inst_with_mtr"]?.ToString(),
                                        dtr_with_wrkng_mtr = reader["dtr_with_wrkng_mtr"]?.ToString(),
                                        per_dtr_wrkng_mtr = reader["per_dtr_wrkng_mtr"]?.ToString(),
                                        per_dtr_mtr = reader["per_dtr_mtr"]?.ToString()
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
    public class KpiDataDTRMETERING
    {
        public string Company { get; set; }
        public string Region { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string dtr_type { get; set; }
        public string total_dtr { get; set; }
        public string dtr_inst_with_mtr { get; set; }
        public string dtr_with_wrkng_mtr { get; set; }
        public string per_dtr_mtr { get; set; }
        public string per_dtr_wrkng_mtr { get; set; }
    }
}
