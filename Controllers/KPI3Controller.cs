using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPI3Controller : ApiController
    {
        [HttpGet]
        [Route("api/kpi3/{company}/{quarter}/{year}/{type}")]
        public IHttpActionResult GetKpiData(string company, string quarter, string year, string type)
        {
            try
            {
                // Ensure parameters are valid
                if (string.IsNullOrEmpty(company) ||
                    string.IsNullOrEmpty(quarter) ||
                    string.IsNullOrEmpty(year) ||
                    string.IsNullOrEmpty(type))
                {
                    var errorMessage = new { error = "Kindly enter valid Company, Quarter, Year, and Type values." };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                // Define the query based on the 'type' parameter
                string query = "";
                if (type.ToLower() == "agri")
                {
                    query = @"
                    SELECT claimed_amt_agri AS claimed_subsidy, claimed_rec_agri AS received_subsidy
                    FROM [discom_d3_subsidy]
                    WHERE id=2  
                    AND  company_name = @company 
                    AND quarter_report = @quarter 
                    AND year_report = @year";
                }
                else if (type.ToLower() == "others")
                {
                    query = @"
                    SELECT claimed_amt_oth AS claimed_subsidy, claimed_rec_oth AS received_subsidy
                    FROM [discom_d3_subsidy]
                    WHERE id=2  
                    AND  company_name = @company
                    AND quarter_report = @quarter 
                    AND year_report = @year";
                }
                else if (type.ToLower() == "total")
                {
                    query = @"
                    SELECT claimed_amt_total AS claimed_subsidy, claimed_rec_total AS received_subsidy
                    FROM [discom_d3_subsidy]
                    WHERE id=2  
                    AND  company_name = @company
                    AND quarter_report = @quarter 
                    AND year_report = @year";
                }
                else
                {
                    var errorMessage = new { error = "Invalid type provided. Use 'agri', 'others', or 'total'." };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                // List to store KPI data
                List<KpiData3> kpiDataList = new List<KpiData3>();

                // Connection string
                string connectionString = ConfigurationManager.ConnectionStrings["constr2"].ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@company", company);
                        command.Parameters.AddWithValue("@quarter", quarter);
                        command.Parameters.AddWithValue("@year", year);

                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                KpiData3 kpiData = new KpiData3
                                {
                                    Company = company,
                                    Quarter = quarter,
                                    Year = year,
                                    claimed_subsidy = reader["claimed_subsidy"]?.ToString(),
                                    received_subsidy = reader["received_subsidy"]?.ToString()
                                };

                                kpiDataList.Add(kpiData);
                            }
                        }
                    }
                }

                // If no data is found
                if (kpiDataList.Count == 0)
                {
                    var errorMessage = new { error = "No KPI data found for the specified parameters." };
                    return Content(HttpStatusCode.NotFound, errorMessage);
                }

                // Return the KPI data as JSON
                return Ok(kpiDataList);
            }
            catch (SqlException)
            {
                var errorMessage = new { error = "An SQL-related error occurred while processing the request." };
                return Content(HttpStatusCode.InternalServerError, errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = new { error = "An internal server error occurred: " + ex.Message };
                return Content(HttpStatusCode.InternalServerError, errorMessage);
            }
        }
    }

    // Model for KPI data
    public class KpiData3
    {
        public string Company { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string claimed_subsidy { get; set; }
        public string received_subsidy { get; set; }
    }
}
