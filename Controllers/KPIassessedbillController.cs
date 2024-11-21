using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPIassessedbillController : ApiController
    {
        [HttpGet]
        [Route("api/KPIassessedbill/{company}/{quarter1}/{year1}/{quarter2}/{year2}")]
        public IHttpActionResult GetKpiData(
            string company,
            string quarter1,
            string year1,
            string quarter2,
            string year2)
        {
            try
            {
                // List to store KPI data
                List<KPIassessedbill> kpiDataList = new List<KPIassessedbill>();

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
                                SUM(CAST(no_of_con_ass_bill AS INT)) AS domestic_cons_bill, 
                                SUM(CAST(no_of_con_mtr_bill AS INT)) AS domestic_cons_bill_mtr, 
                                SUM(CAST(unit_mu_con_ass_bill AS FLOAT)) AS units_bill_assmt,
                                SUM(CAST(unit_mu_con_mtr_bill AS FLOAT)) AS units_bill_a_mtr
                            FROM discom_d7_assessedbilling
                            WHERE quarter_report = @quarter
                              AND year_report = @year
                              AND company_name = @company
                              AND ISNUMERIC(no_of_con_ass_bill) = 1;";

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
                                    KPIassessedbill kpiData = new KPIassessedbill
                                    {
                                        Company = company,
                                        Quarter = pair.Key,
                                        Year = pair.Value,
                                        domestic_cons_bill = reader["domestic_cons_bill"]?.ToString(),
                                        domestic_cons_bill_mtr = reader["domestic_cons_bill_mtr"]?.ToString(),
                                        units_bill_assmt = reader["units_bill_assmt"]?.ToString(),
                                        units_bill_a_mtr = reader["units_bill_a_mtr"]?.ToString(),
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
    public class KPIassessedbill
    {
        public string Company { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string domestic_cons_bill { get; set; }
        public string domestic_cons_bill_mtr { get; set; }
        public string units_bill_assmt { get; set; }
        public string units_bill_a_mtr { get; set; }
    }
}
