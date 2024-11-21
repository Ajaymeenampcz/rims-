using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPIsmartmeteringController : ApiController
    {
        private readonly string _connectionString;

        public KPIsmartmeteringController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["constr2"].ToString();
        }

        [HttpGet]
        [Route("api/KPIsmartmetering/{year}")]
        public IHttpActionResult GetKpiData(string year)
        {
            // Validate year parameter
            if (string.IsNullOrEmpty(year))
            {
                return BadRequest("Kindly enter a valid year value");
            }

            try
            {
                var kpiDataList = FetchKpiData(year);

                // If no data is found
                if (kpiDataList.Count == 0)
                {
                    return Content(HttpStatusCode.NotFound, "No records found for the specified financial year");
                }

                // Return the KPI data as JSON
                return Ok(kpiDataList);
            }
            catch (SqlException)
            {
                return InternalServerError(new Exception("An SQL-related error occurred while processing the request."));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("An internal server error occurred: " + ex.Message));
            }
        }

        private List<KPIsmartmetering> FetchKpiData(string year)
        {
            // SQL query to fetch KPI data
            string query = @"
               SELECT
                   finyear_report,

                   -- EZ totals (ensuring category is 'EZ')
                   SUM(CASE WHEN category LIKE '%EZ%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END) AS ez_total_targeted_nos,
                   SUM(CASE WHEN category LIKE '%EZ%' THEN CAST(ISNULL(total_no_existing_replcd, '0') AS FLOAT) ELSE 0 END) AS ez_total_achieved_nos,
                   CASE
                       WHEN SUM(CASE WHEN category LIKE '%EZ%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END) > 0
                       THEN ROUND(
                           SUM(CASE WHEN category LIKE '%EZ%' THEN CAST(ISNULL(total_no_existing_replcd, '0') AS FLOAT) ELSE 0 END) * 100.0 /
                           SUM(CASE WHEN category LIKE '%EZ%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END), 2)
                       ELSE 0
                   END AS ez_percentage_target_achieved,

                   -- WZ totals (ensuring category is 'Total WZ')
                   SUM(CASE WHEN category LIKE '%Total WZ%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END) AS wz_total_targeted_nos,
                   SUM(CASE WHEN category LIKE '%Total WZ%' THEN CAST(ISNULL(total_no_existing_replcd, '0') AS FLOAT) ELSE 0 END) AS wz_total_achieved_nos,
                   CASE
                       WHEN SUM(CASE WHEN category LIKE '%Total WZ%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END) > 0
                       THEN ROUND(
                           SUM(CASE WHEN category LIKE '%Total WZ%' THEN CAST(ISNULL(total_no_existing_replcd, '0') AS FLOAT) ELSE 0 END) * 100.0 /
                           SUM(CASE WHEN category LIKE '%Total WZ%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END), 2)
                       ELSE 0
                   END AS wz_percentage_target_achieved,

                   -- CZ totals (ensuring category is 'CZ Total')
                   SUM(CASE WHEN category LIKE '%CZ Total%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END) AS cz_total_targeted_nos,
                   SUM(CASE WHEN category LIKE '%CZ Total%' THEN CAST(ISNULL(total_no_existing_replcd, '0') AS FLOAT) ELSE 0 END) AS cz_total_achieved_nos,
                   CASE
                       WHEN SUM(CASE WHEN category LIKE '%CZ Total%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END) > 0
                       THEN ROUND(
                           SUM(CASE WHEN category LIKE '%CZ Total%' THEN CAST(ISNULL(total_no_existing_replcd, '0') AS FLOAT) ELSE 0 END) * 100.0 /
                           SUM(CASE WHEN category LIKE '%CZ Total%' THEN CAST(target_repl_in_fin_year AS FLOAT) ELSE 0 END), 2)
                       ELSE 0
                   END AS cz_percentage_target_achieved

               FROM
                   annual_discom_d11_smartmetering
               WHERE
                   finyear_report = @year
               GROUP BY
                   finyear_report";

            List<KPIsmartmetering> kpiDataList = new List<KPIsmartmetering>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@year", year);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            KPIsmartmetering kpiData = new KPIsmartmetering
                            {
                                Year = reader["finyear_report"]?.ToString(),
                                EZ = new EZData
                                {
                                    total_targeted_nos = reader["ez_total_targeted_nos"]?.ToString(),
                                    total_achieved_nos = reader["ez_total_achieved_nos"]?.ToString(),
                                    percentage_target_achieved = reader["ez_percentage_target_achieved"]?.ToString()
                                },
                                WZ = new WZData
                                {
                                    total_targeted_nos = reader["wz_total_targeted_nos"]?.ToString(),
                                    total_achieved_nos = reader["wz_total_achieved_nos"]?.ToString(),
                                    percentage_target_achieved = reader["wz_percentage_target_achieved"]?.ToString()
                                },
                                CZ = new CZData
                                {
                                    total_targeted_nos = reader["cz_total_targeted_nos"]?.ToString(),
                                    total_achieved_nos = reader["cz_total_achieved_nos"]?.ToString(),
                                    percentage_target_achieved = reader["cz_percentage_target_achieved"]?.ToString()
                                }
                            };

                            kpiDataList.Add(kpiData);
                        }
                    }
                }
            }
            return kpiDataList;
        }
    }

    // Model for KPI data
    public class KPIsmartmetering
    {
        public string Year { get; set; }
        public EZData EZ { get; set; }
        public WZData WZ { get; set; }
        public CZData CZ { get; set; }
    }

    public class EZData
    {
        public string total_targeted_nos { get; set; }
        public string total_achieved_nos { get; set; }
        public string percentage_target_achieved { get; set; }
    }

    public class WZData
    {
        public string total_targeted_nos { get; set; }
        public string total_achieved_nos { get; set; }
        public string percentage_target_achieved { get; set; }
    }

    public class CZData
    {
        public string total_targeted_nos { get; set; }
        public string total_achieved_nos { get; set; }
        public string percentage_target_achieved { get; set; }
    }
}
