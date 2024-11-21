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
    public class KPI1comparisonController : ApiController
    {
        [HttpGet]
        [Route("api/kpi1comparison/{company}/{region}/{periodicity}/{quarter1?}/{year1}/{quarter2?}/{year2?}/{quarter3?}/{year3?}/{quarter4?}/{year4?}/{metric}")]
        public IHttpActionResult GetKpiData(
     string company, string region, string periodicity,
     string quarter1, string year1,
     string quarter2, string year2,
     string quarter3, string year3,
     string quarter4, string year4,
     string metric)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(metric) || string.IsNullOrEmpty(periodicity))
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Please provide valid region, periodicity, and metric values." });
                }

                List<KpiDatacomparison> kpiDataList = new List<KpiDatacomparison>();
                string connectionString = ConfigurationManager.ConnectionStrings["constr2"].ToString();

                // Define region mapping
                List<string> regionList = GetRegionList(region);
                if (regionList == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Invalid region" });
                }

                // Build dynamic region condition
                string regionCondition = string.Join(",", regionList.Select(r => $"'{r}'"));

                // Select the appropriate metric column based on the passed metric
                string selectedColumn = GetMetricColumn(metric);
                if (string.IsNullOrEmpty(selectedColumn))
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Invalid metric" });
                }

                // Logic for periodicity types
                if (periodicity.Equals("quarterly", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle quarterly data
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

                    // Add quarter-year combinations from input parameters
                    AddQuarterYear(quarter1, year1);
                    AddQuarterYear(quarter2, year2);
                    AddQuarterYear(quarter3, year3);
                    AddQuarterYear(quarter4, year4);

                    if (quarterYearPairs.Count == 0)
                    {
                        return Content(HttpStatusCode.BadRequest, new { error = "Please provide at least one valid Quarter and Year combination for quarterly data." });
                    }

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        foreach (var pair in quarterYearPairs)
                        {
                            string query = $@"
                    SELECT 
                        region, 
                        {selectedColumn}
                    FROM 
                        [discom_d1_kpi]
                    WHERE 
                        company_name = @company 
                        AND quarter_report = @quarter 
                        AND year_report = @year
                        AND region IN ({regionCondition}) 
                    GROUP BY 
                        region";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@company", company);
                                command.Parameters.AddWithValue("@quarter", pair.Key);
                                command.Parameters.AddWithValue("@year", pair.Value);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var kpiData = new KpiDatacomparison
                                        {
                                            Company = company,
                                            Region = reader["region"].ToString(),
                                            Quarter = pair.Key,
                                            Year = pair.Value,
                                            Value = reader[metric]?.ToString() ?? "N/A"  // Metric value dynamically mapped
                                        };
                                        kpiDataList.Add(kpiData);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (periodicity.Equals("annual", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle annual data
                    List<string> years = new List<string> { year1, year2, year3, year4 }
                        .Where(y => !string.IsNullOrEmpty(y) && y != "0") // Only include valid years
                        .ToList();

                    // Validate that exactly 4 years are provided
                    //if (years.Count != 4)
                    //{
                    //    return Content(HttpStatusCode.BadRequest, new { error = "Please provide exactly 4 valid years for annual data." });
                    //}

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        foreach (var year in years)
                        {
                            string query = $@"
                    SELECT 
                        region, 
                        {selectedColumn}
                    FROM 
                        [annual_discom_d1_kpi]
                    WHERE 
                        company_name = @company 
                        AND finyear_report = @year
                        AND region IN ({regionCondition}) 
                    GROUP BY 
                        region";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@company", company);
                                command.Parameters.AddWithValue("@year", year);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var kpiData = new KpiDatacomparison
                                        {
                                            Company = company,
                                            Region = reader["region"].ToString(),
                                            Quarter = null,  // No quarter for annual data
                                            Year = year,
                                            Value = reader[metric]?.ToString() ?? "N/A"  // Metric value dynamically mapped
                                        };
                                        kpiDataList.Add(kpiData);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, new { error = "Invalid periodicity value. It should be either 'quarterly' or 'annual'." });
                }

                if (kpiDataList.Count == 0)
                {
                    return Content(HttpStatusCode.NotFound, new { error = "No KPI data found for the specified parameters" });
                }

                return Ok(kpiDataList);
            }
            catch (SqlException ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { error = "An SQL-related error occurred: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { error = "An internal server error occurred: " + ex.Message });
            }
        }


        // Helper methods
        private List<string> GetRegionList(string region)
        {
            switch (region.ToUpper())
            {
                case "CENTRAL DISCOM":
                    return new List<string> { "CENTRAL DISCOM" };
                case "EAST DISCOM":
                    return new List<string> { "EAST DISCOM" };
                case "WEST DISCOM":
                    return new List<string> { "WEST DISCOM" };
                default:
                    return new List<string> { region.ToUpper() };
            }
        }

        private string GetMetricColumn(string metric)
        {
            switch (metric.ToLower())
            {
                case "totalinput":
                    return "MAX(CASE WHEN particulars IN ('Total_Input', 'Total Input') THEN For_the_Current_qtr END) AS TotalInput";
                case "totalsales":
                    return "MAX(CASE WHEN particulars IN ('Total_Sales_total', 'Total Sales (2 3)') THEN For_the_Current_qtr END) AS TotalSales";
                case "totaldemand":
                    return "MAX(CASE WHEN particulars IN ('Total_Demand', 'Total Demand (5 6)') THEN For_the_Current_qtr END) AS TotalDemand";
                case "totalcollection":
                    return "MAX(CASE WHEN particulars IN ('Total_Collections', 'Total Collections (8 9)') THEN For_the_Current_qtr END) AS TotalCollection";
                default:
                    return null;
            }
        }
    }

    // Model for KPI data
    public class KpiDatacomparison
    {
        public string Company { get; set; }
        public string Region { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string Value { get; set; } // Selected metric value
    }
}
