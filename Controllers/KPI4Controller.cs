using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPI4Controller : ApiController
    {
        [HttpGet]
        [Route("api/kpi4/{company}/{region}/{quarter}/{year}")]
        public IHttpActionResult GetKpiData(string company, string region, string quarter, string year)
        {
            try
            {
                // Ensure parameters are valid
                if (string.IsNullOrEmpty(company) ||
                    string.IsNullOrEmpty(region) ||
                    string.IsNullOrEmpty(quarter) ||
                    string.IsNullOrEmpty (year))
                  
                {
                    var errorMessage = new { error = "Kindly enter valid Company, Region, Quarter, Year values." };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                // Define the query based on the 'type' parameter
                // Region mapping
                List<string> regionList = new List<string>();
                if (region == "CENTRAL DISCOM")
                {
                    regionList = new List<string> { "Bhopal", "Gwalior", "Discom" };
                }
                else if (region == "EAST DISCOM")
                {
                    regionList = new List<string> { "Jabalpur", "Rewa", "Sagar", "Shahdol", "Total" };
                }
                else if (region == "WEST DISCOM")
                {
                    regionList = new List<string> { "Indore", "Ujjain", "WZ" };
                }
                else
                {
                    regionList.Add(region);
                }

                // Dynamically build region condition for the query
                string regionCondition = string.Join(",", regionList.Select(r => $"'{r}'"));

                // SQL query with dynamic region condition
                string query = $@"
              select region,no_of_lt_cons_end_qtr_r,un_mtr_beg_of_qtr_r,per_un_mtrd_r from discom_d4lt_unmetered
                  where  company_name = @company 
                    AND quarter_report = @quarter 
                    AND year_report = @year
                    AND region IN ({regionCondition}) 
               ";

                // List to store KPI data
                List<KpiData4> kpiDataList = new List<KpiData4>();

                // Connection string
                string connectionString = ConfigurationManager.ConnectionStrings["constr2"].ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@company", company);
                        command.Parameters.AddWithValue("@quarter", quarter);
                        command.Parameters.AddWithValue("@region", region);
                        command.Parameters.AddWithValue("@year", year);

                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                KpiData4 KpiData4 = new KpiData4
                                {
                                    Company = company,
                                    Region = reader["region"].ToString(),
                                    Quarter = quarter,
                                    Year = year,
                                    no_of_lt_cons_r = reader["no_of_lt_cons_end_qtr_r"]?.ToString(),
                                    un_mtr_r = reader["un_mtr_beg_of_qtr_r"]?.ToString(),
                                    per_un_mtr_r = reader["per_un_mtrd_r"]?.ToString(),
                                    
                                };

                                kpiDataList.Add(KpiData4);
                            }
                        }
                    }
                }

                // If no data is found
                if (kpiDataList.Count == 0)
                {
                    var errorMessage = new { error = "No KPI data found for the specified parameters" };
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
    public class KpiData4
    {
        public string Company { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string Region { get; set; }
        
        public string no_of_lt_cons_r { get; set; }
        public string un_mtr_r { get; set; }
        public string per_un_mtr_r{ get; set; }
    }
}
