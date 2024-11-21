using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPIdefectiveController : ApiController
    {
        [HttpGet]
        [Route("api/KPIdefective/{company}/{quarter1}/{year1}/{quarter2}/{year2}")]
        public IHttpActionResult GetKpiData(
            string company,
           // string circle,
            string quarter1,
            string year1,
            string quarter2,
            string year2)
        {
            try
            {
               

                // List to store KPI data
                List<KPIdefective> kpiDataList = new List<KPIdefective>();

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
                                SUM(CAST(no_lt_mtr_cons_end_qtr_ttl AS INT)) AS no_lt_mtr_cons_end_qtr_ttl, 
                                SUM(CAST(no_stp_def_beg_qtr_ttl AS INT)) AS no_stp_def_beg_qtr_ttl, 
                                SUM(CAST(per_def_mtrd_cons_endqtr AS float)) AS per_def_mtrd_cons_endqtr
                            FROM discom_d5lt_defect_metered
                            WHERE quarter_report = @quarter
                              AND year_report = @year
                              AND company_name = @company
                              AND ISNUMERIC(no_stp_def_beg_qtr_ttl) = 1;";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Add parameters
                            command.Parameters.AddWithValue("@company", company);
                            command.Parameters.AddWithValue("@quarter", pair.Key); // Quarter
                            command.Parameters.AddWithValue("@year", pair.Value); // Year

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    KPIdefective kpiData = new KPIdefective
                                    {
                                        Company = company,
                                       // Region = circle,
                                        Quarter = pair.Key,
                                        Year = pair.Value,
                                        no_lt_mtr_cons_end_qtr_ttl = reader["no_lt_mtr_cons_end_qtr_ttl"]?.ToString(),
                                        no_stp_def_beg_qtr_ttl = reader["no_stp_def_beg_qtr_ttl"]?.ToString(),
                                        per_def_mtrd_cons_endqtr = reader["per_def_mtrd_cons_endqtr"]?.ToString(),
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
    public class KPIdefective
    {
        public string Company { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string no_lt_mtr_cons_end_qtr_ttl { get; set; }
        public string no_stp_def_beg_qtr_ttl { get; set; }
        public string per_def_mtrd_cons_endqtr { get; set; }
    }
}
