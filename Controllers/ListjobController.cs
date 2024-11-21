using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class ListjobController : ApiController
    {
        [HttpGet]
        [Route("api/listjob/{empCode}")]
        public IHttpActionResult GetListjobData(string empCode)
        {
            try
            {
                string RegionName = "";
                string CircleName = "";
                string DivisionName = "";
                string DcName = "";
                string SsName = "";

                string query = "SELECT job_no,region_code,circle_code,division_code,dc_code, ss_code FROM SS_JOBENTRY WHERE updatedby_empcode = @EmpCode";

                if (string.IsNullOrEmpty(empCode))
                {
                    // Return a 400 Bad Request status with a JSON error message
                    var errorMessage = new { error = "Kindly enter Employee code" };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                List<Joblist> jobList = new List<Joblist>();

                string connStr = ConfigurationManager.ConnectionStrings["constr"].ToString();

                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmpCode", empCode);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //string connectionString1 = ConfigurationManager.ConnectionStrings["constr1"].ToString();

                                using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["constr1"].ToString()))
                                {
                                    connection1.Open();

                                    RegionName = GetScalarValue(connection1, "SELECT region_name FROM master_region WHERE region_code = " + reader["region_code"].ToString());
                                    CircleName = GetScalarValue(connection1, "SELECT circle_name FROM master_circle WHERE circle_code = " + reader["circle_code"].ToString());
                                    DivisionName = GetScalarValue(connection1, "SELECT division_name FROM master_division WHERE division_code = " + reader["division_code"].ToString());
                                    DcName = GetScalarValue(connection1, "SELECT dc_name FROM master_dc WHERE dc_code = " + reader["dc_code"].ToString());
                                    SsName = GetScalarValue(connection1, "SELECT ss_name FROM master_feeder WHERE ss_code = " + reader["ss_code"].ToString());

                                    connection1.Close();
                                }

                                Joblist job = new Joblist
                                {
                                    job_no = reader["job_no"].ToString(),
                                    region_code = Convert.ToInt64(reader["region_code"].ToString()),
                                    region_name = RegionName.ToString(),
                                    circle_code = Convert.ToInt64(reader["circle_code"].ToString()),
                                    circle_name = CircleName.ToString(),
                                    division_code = Convert.ToInt64(reader["division_code"].ToString()),
                                    division_name = DivisionName.ToString(),
                                    dc_code = Convert.ToInt64(reader["dc_code"].ToString()),
                                    dc_name = DcName.ToString(),
                                    ss_code = Convert.ToInt64(reader["ss_code"].ToString()),
                                    ss_name = SsName.ToString(),

                                };

                                jobList.Add(job);
                            }
                        }
                    }
                }

                if (jobList.Count == 0)
                {
                    // Return a 404 Not Found status with a JSON error message
                    var errorMessage = new { error = "No jobs found for the provided employee code" };
                    return Content(HttpStatusCode.NotFound, errorMessage);
                }

                return Ok(jobList); // Return the data as JSON
            }
            catch (SqlException ex)
            {
                // Handle SQL-related exceptions and return a 500 Internal Server Error with a JSON error message
                var errorMessage = new { error = "An SQL-related server error occurred while processing the request." };
                return Content(HttpStatusCode.InternalServerError, errorMessage);
            }
            catch (ArgumentException ex)
            {
                // Handle ArgumentException and return a 400 Bad Request with a JSON error message
                var errorMessage = new { error = "Bad request: " + ex.Message };
                return Content(HttpStatusCode.BadRequest, errorMessage);
            }
            catch (Exception ex)
            {
                // Handle other exceptions and return a 500 Internal Server Error with a JSON error message
                var errorMessage = new { error = "An internal server error occurred while processing the request." };
                return Content(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        private string GetScalarValue(SqlConnection connection, string query)
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    return result.ToString();
                }
                return string.Empty;
            }
        }
    }


    public class Joblist
    {
        public string job_no { get; set; }

        public Int64 region_code { get; set; }
        public string region_name { get; set; }

        public Int64 circle_code { get; set; }
        public string circle_name { get; set; }

        public Int64 division_code { get; set; }
        public string division_name { get; set; }

        public Int64 dc_code { get; set; }
        public string dc_name { get; set; }

        public Int64 ss_code { get; set; }
        public string ss_name { get; set; }
    }
}
