using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPI1Controller : ApiController
    {
        [HttpGet]
        [Route("api/kpi1/{company}/{region}/{quarter}/{year}")]
        public IHttpActionResult GetKpiData(string company, string region, string quarter, string year)
        {
            try
            {
                // Ensure parameters are valid
                if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(quarter) || string.IsNullOrEmpty(year))
                {
                    var errorMessage = new { error = "Kindly enter valid region, quarter, and year values" };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                // Region mapping
                List<string> regionList = new List<string>();
                if (region == "CENTRAL DISCOM")
                {
                    regionList = new List<string> { "BHOPAL", "GWALIOR", "CENTRAL DISCOM" };
                }
                else if (region == "EAST DISCOM")
                {
                    regionList = new List<string> { "JABALPUR", "REWA", "SAGAR", "SHAHDOL", "EAST DISCOM" };
                }
                else if (region == "WEST DISCOM")
                {
                    regionList = new List<string> { "INDORE", "UJJAIN", "WEST DISCOM" };
                }
                else
                {
                    regionList.Add(region);
                }

                // Dynamically build region condition for the query
                string regionCondition = string.Join(",", regionList.Select(r => $"'{r}'"));

                // SQL query with dynamic region condition
                string query = $@"
                SELECT 
                    region,
                    MAX(CASE WHEN particulars IN ('Total_Input', 'Total Input') THEN For_the_Current_qtr END) AS Total_Input,
                    MAX(CASE WHEN particulars IN ('Total_Sales_total', 'Total Sales (2 3)') THEN For_the_Current_qtr END) AS Total_Input,
                    MAX(CASE WHEN particulars IN ('Total_Demand', 'Total Demand (5 6)') THEN For_the_Current_qtr END) AS Total_Demand,
                    MAX(CASE WHEN particulars IN ('Total_Collections', 'Total Collections (8 9)') THEN For_the_Current_qtr END) AS Total_Collection
                FROM 
                    [discom_d1_kpi]
                WHERE 
                    particulars IN (
                        'Total_Input', 'Total Input', 
                        'Total_Sales_total', 'Total Sales (2 3)', 
                        'Total_Demand', 'Total Demand (5 6)', 
                        'Total_Collections', 'Total Collections (8 9)'
                    ) 
                    AND company_name = @company 
                    AND quarter_report = @quarter 
                    AND year_report = @year
                    AND region IN ({regionCondition}) 
                GROUP BY 
                    region";

                // List to store KPI data
                List<KpiData> kpiDataList = new List<KpiData>();

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
                                KpiData kpiData = new KpiData
                                {
                                    Company = company,
                                    Region = reader["region"].ToString(),
                                    Quarter = quarter,
                                    Year = year,
                                    TotalInput = reader["Total_Input"]?.ToString(),
                                    TotalSales = reader["Total_Sales"]?.ToString(),
                                    TotalDemand = reader["Total_Demand"]?.ToString(),
                                    TotalCollection = reader["Total_Collection"]?.ToString()
                                };

                                kpiDataList.Add(kpiData);
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
    public class KpiData
    {
        public string Company { get; set; }
        public string Region { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string TotalInput { get; set; }
        public string TotalSales { get; set; }
        public string TotalDemand { get; set; }
        public string TotalCollection { get; set; }
    }
}
