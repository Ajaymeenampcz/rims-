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
    public class KPI2Controller : ApiController
    {
        [HttpGet]
        [Route("api/kpi2/{company}/{regions}/{quarter}/{year}")]
        public IHttpActionResult GetKpiData(string company, string regions, string quarter, string year)
        {
            try
            {
                // Ensure parameters are valid
                if (string.IsNullOrEmpty(regions) || string.IsNullOrEmpty(quarter) || string.IsNullOrEmpty(year))
                {
                    var errorMessage = new { error = "Kindly enter valid regions, quarter, and year values" };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                // Split the regions by comma and trim whitespace
                var regionList = regions.Split(',').Select(r => r.Trim()).ToList();

                // Dynamically build region condition for the query
                string regionCondition = string.Join(",", regionList.Select(r => $"'{r}'"));

                // SQL query with dynamic region condition for billingeff, collectioneff, distributionloss, and ATnCloss
                string query = $@"
                    SELECT 
                        region,
                        MAX(CASE WHEN particulars IN ('AT&C Loss','AT&C Loss (1-(4/1)*(10/7)) x 100 %','AT&C_Loss_x_100_%','ATnC_Loss_x_100_%','AT') THEN CAST(For_the_Current_qtr AS FLOAT) END) AS ATnCloss,
                        MAX(CASE WHEN particulars IN ('Distribution Loss ((1-4)/1) x 100%', 'Distribution_Loss_x_100%') THEN CAST(For_the_Current_qtr AS FLOAT) END) AS distributionloss,
                        (100 - MAX(CASE WHEN particulars IN ('Distribution Loss ((1-4)/1) x 100%', 'Distribution_Loss_x_100%') THEN CAST(For_the_Current_qtr AS FLOAT) END)) AS billingeff,
                        MAX(CASE WHEN particulars IN ('Total collection efficiency (10/7) x 100%', 'Total_collection_efficiency_x_100%') THEN CAST(For_the_Current_qtr AS FLOAT) END) AS collectioneff
                    FROM 
                        [discom_d1_kpi]
                    WHERE 
                        particulars IN (
                            'AT&C Loss', 'AT&C Loss (1-(4/1)*(10/7)) x 100 %', 'AT&C_Loss_x_100_%', 'ATnC_Loss_x_100_%', 'AT',
                            'Distribution Loss ((1-4)/1) x 100%', 'Distribution_Loss_x_100%',
                            'Total collection efficiency (10/7) x 100%', 'Total_collection_efficiency_x_100%'
                        )
                        AND company_name = @company
                        AND quarter_report = @quarter
                        AND year_report = @year
                        AND region IN ({regionCondition})
                    GROUP BY 
                        region;";

                // List to store KPI data
                List<Kpi2> kpiDataList = new List<Kpi2>();

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
                                Kpi2 kpiData = new Kpi2
                                {
                                    Company = company,
                                    Region = reader["region"].ToString(),
                                    Quarter = quarter,
                                    Year = year,
                                    ATnCloss = reader["ATnCloss"]?.ToString(),
                                    DistributionLoss = reader["distributionloss"]?.ToString(),
                                    BillingEfficiency = reader["billingeff"]?.ToString(),
                                    CollectionEfficiency = reader["collectioneff"]?.ToString()
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
    public class Kpi2
    {
        public string Company { get; set; }
        public string Region { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string ATnCloss { get; set; }
        public string DistributionLoss { get; set; }
        public string BillingEfficiency { get; set; }
        public string CollectionEfficiency { get; set; }
    }
}
