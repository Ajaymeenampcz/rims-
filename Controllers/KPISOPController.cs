using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class KPISOPController : ApiController
    {
        [HttpGet]
        [Route("api/kpisop/{company}/{quarter}/{year}/{type}")]
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
                if (type.ToLower() == "normal_fuse")
                        {
                            query = @"
    SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop

    WHERE [service] IN ('Responding to Normal Fuse -off Call and rectifications', 'Responding to Normal Fuse -off Call and rectificat')
    AND [service_desc] IN ('Urban areas', 'Rural areas')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
   GROUP BY
        [per_achieved_in_limit]";
                        }
                        else if (type.ToLower() == "line_breakdown")
                        {
                    query = @"
    SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop
    
    WHERE [service] IN ('Restoration of supply on account of Line Breakdown (not including breaking /uprooting of poles )  ', 'Restoration of supply on account of Line Breakdown (not including breaking /uprooting of poles )  ')
    AND [service_desc] IN ('Urban areas', 'Rural areas')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
GROUP BY
        [per_achieved_in_limit]";
                        }
                        else if (type.ToLower() == "transformer_failure")
                        {
                            query = @"
    SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop
    
    WHERE [service] IN ('Distribution Transformer failure', 'Distribution Transformer failure')
    AND [service_desc] IN ('Replacement of transformer or restoration of supply in Commissionary head quarter', 
                           'Replacement of transformer or restoration of supply in urban areas other than Commissionary head quarter',
                           'Replacement of transformer or restoration of supply in rural areas')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
GROUP BY
        [per_achieved_in_limit]";
                        }
                        else if (type.ToLower() == "meter_complaints")
                        {
                            query = @"
    SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop
 
    WHERE [service] IN ('Meter Complaints', 'Meter Complaints')
    AND [service_desc] IN ('Inspect and check correctness', 'Replace slow, creeping or stuck up meters',
                           'Replace burnt meters if cause not attributed to consumer', 'Replace burnt meters in all other cases')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
GROUP BY
        [per_achieved_in_limit]";
                        }
                        else if (type.ToLower() == "bill_complaints")
                        {
                            query = @"
    SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop
    WHERE [service] IN ('Resolution of complaints on consumer’s bills ', 'Resolution of complaints on consumer’s bills ', 'Resolution of complaints on consumer’s bills ', 'Resolution of complaints on consumer’s bills ')
    AND [service_desc] IN ('If additional information is required to be collec ', 'If additional information is required to be collected', 'If no additional information is required', 'Time period within which bills are to be served;')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
GROUP BY
        [per_achieved_in_limit]";
                        }
                        else if (type.ToLower() == "reconnection")
                        {
                            query = @"
   SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop
    WHERE [service] IN ('Reconnection of supply following disconnection', 'Reconnection of supply following disconnection')
    AND [service_desc] IN ('Towns and cities', 'Rural areas')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
GROUP BY
        [per_achieved_in_limit]";
                        }
                        else if (type.ToLower() == "new_connection")
                        {
                            query = @"
    SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop
    WHERE [service] IN ('Application for new connection/enhancement of contract demand/reduction in contract demand', 'Application for new connection/enhancement of contract demand/reduction in contract demand')
    AND [service_desc] IN ('Deviation from target in case of LT', 'Deviation from target in case of HT and EHT')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
GROUP BY
        [per_achieved_in_limit]";
                        }
                        else if (type.ToLower() == "service_conversion")
                        {
                            query = @"
    SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop
    WHERE [service] IN ('Conversion of service', 'Conversion of service')
    AND [service_desc] IN ('Change of category', 'Conversion from LT 1-ph to LT 3-ph and vice-versa', 'Time taken for change in consumer details;')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
GROUP BY
        [per_achieved_in_limit]";
                        }
                        else if (type.ToLower() == "bill_serving")
                        {
                            query = @"
   SELECT 
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_inst_occured]), 0)) AS total_inst_occured,
    SUM(COALESCE(TRY_CONVERT(INT, [no_of_cases_ach_in_limit]), 0)) AS total_cases_ach_in_limit,
    [per_achieved_in_limit]
FROM discom_d20_sop
    WHERE [service] IN ('Time period within which bills are to be served;', 'Time period within which bills are to be served;')
    AND company_name = @company
    AND quarter_report = @quarter
    AND year_report = @year
GROUP BY
        [per_achieved_in_limit]";
                        }
                        else
                        {
                            var errorMessage = new { error = "Invalid type provided. Use 'normal_fuse', 'line_breakdown', 'transformer_failure', 'meter_complaints', 'bill_complaints', 'reconnection', 'new_connection', 'service_conversion', or 'bill_serving' '." };
                            return Content(HttpStatusCode.BadRequest, errorMessage);
                        }

                        // List to store KPI data
                        List<KPISOP> kpiDataList = new List<KPISOP>();

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
                        command.Parameters.AddWithValue("@type", type);

                                connection.Open();

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                KPISOP kpiData = new KPISOP
                                {
                                    Company = company,
                                    Quarter = quarter,
                                    Year = year,
                                    no_of_inst_occured = reader["total_inst_occured"] != DBNull.Value ? reader["total_inst_occured"].ToString() : "0",
                                    no_of_cases_ach_in_limit = reader["total_cases_ach_in_limit"] != DBNull.Value ? reader["total_cases_ach_in_limit"].ToString() : "0",
                                    per_achieved_in_limit = reader["per_achieved_in_limit"] != DBNull.Value ? reader["per_achieved_in_limit"].ToString() : "0"

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
            public class KPISOP
    {
                public string Company { get; set; }
                public string Quarter { get; set; }
                public string Year { get; set; }
                public string no_of_inst_occured { get; set; }
                public string no_of_cases_ach_in_limit { get; set; }
                public string per_achieved_in_limit { get; set; }
            }
        }

                