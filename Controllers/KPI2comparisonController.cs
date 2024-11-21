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
    public class KPI2comparisonController : ApiController
    {
        [HttpGet]
        [Route("api/KPI2comparison/{company}/{region}/{periodicity}/{quarter1?}/{year1}/{quarter2?}/{year2?}/{quarter3?}/{year3?}/{quarter4?}/{year4?}/{metric}")]
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

                var kpiDataList = new List<KPI2comparison>();
                string connectionString = ConfigurationManager.ConnectionStrings["constr2"].ToString();

                // Define region mapping
                var regionList = GetRegionList(region);
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
                if (string.Equals(periodicity, "quarterly", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle quarterly data
                    var quarterYearPairs = new Dictionary<string, string>();

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

                    using (var connection = new SqlConnection(connectionString))
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

                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@company", company);
                                command.Parameters.AddWithValue("@quarter", pair.Key);
                                command.Parameters.AddWithValue("@year", pair.Value);

                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var kpiData = new KPI2comparison
                                        {
                                            Company = company,
                                            Region = reader["region"].ToString(),
                                            Quarter = pair.Key,
                                            Year = pair.Value,
                                            Value = reader[selectedColumn]?.ToString() ?? "N/A"
                                        };
                                        kpiDataList.Add(kpiData);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (string.Equals(periodicity, "annual", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle annual data
                    var years = new List<string> { year1, year2, year3, year4 }
                        .Where(y => !string.IsNullOrEmpty(y) && y != "0")
                        .ToList();

                    if (years.Count == 0)
                    {
                        return Content(HttpStatusCode.BadRequest, new { error = "Please provide at least one valid year for annual data." });
                    }

                    using (var connection = new SqlConnection(connectionString))
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

                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@company", company);
                                command.Parameters.AddWithValue("@year", year);

                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var kpiData = new KPI2comparison
                                        {
                                            Company = company,
                                            Region = reader["region"].ToString(),
                                            Quarter = null,
                                            Year = year,
                                            Value = reader[selectedColumn]?.ToString() ?? "N/A"
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
            switch (region.ToUpperInvariant())
            {
                case "CENTRAL DISCOM":
                    return new List<string> { "CENTRAL DISCOM" };
                case "EAST DISCOM":
                    return new List<string> { "EAST DISCOM" };
                case "WEST DISCOM":
                    return new List<string> { "WEST DISCOM" };
                default:
                    return new List<string> { region.ToUpperInvariant() };
            }
        }

        private string GetMetricColumn(string metric)
        {
            switch (metric.ToLowerInvariant())
            {
                case "billingeff":
                    return "100 - MAX(CASE WHEN particulars IN ('Distribution Loss ((1-4)/1) x 100%', 'Distribution_Loss_x_100%') THEN CAST(For_the_Current_qtr AS FLOAT) END) AS billingeff";
                case "collectioneff":
                    return "MAX(CASE WHEN particulars IN ('Total collection efficiency (10/7) x 100%', 'Total_collection_efficiency_x_100%') THEN CAST(For_the_Current_qtr AS FLOAT) END) AS collectioneff";
                case "distributionloss":
                    return "MAX(CASE WHEN particulars IN ('Distribution Loss ((1-4)/1) x 100%', 'Distribution_Loss_x_100%') THEN CAST(For_the_Current_qtr AS FLOAT) END) AS distributionloss";
                case "atncloss":
                    return "MAX(CASE WHEN particulars IN ('AT&C Loss', 'AT&C Loss (1-(4/1)*(10/7)) x 100 %', 'AT&C_Loss_x_100_%') THEN CAST(For_the_Current_qtr AS FLOAT) END) AS atncloss";
                default:
                    return null;
            }
        }
    }

    public class KPI2comparison
    {
        public string Company { get; set; }
        public string Region { get; set; }
        public string Quarter { get; set; }
        public string Year { get; set; }
        public string Value { get; set; }
    }
}
