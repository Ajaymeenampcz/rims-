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
    public class SsController : ApiController
    {

        [HttpGet]
        [Route("api/ss/{dcCode}")]
        public IHttpActionResult GetSsData(string DcCode)
        {
            try
            {
                string query = "SELECT DISTINCT ss_code,ss_name from master_feeder where dc_code = @DcCode";

                if (@DcCode == "")
                {
                    // Return a 404 Not Found status with a JSON error message
                    var errorMessage = new { error = "Kindly enter DC code" };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                List<Ss> Ss = new List<Ss>();

                string connectionString = ConfigurationManager.ConnectionStrings["constr1"].ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DcCode", DcCode);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Ss ss = new Ss
                                {
                                    SsCode = reader["ss_code"].ToString(),
                                    SsName = reader["ss_name"].ToString()
                                };

                                Ss.Add(ss);
                            }
                        }
                    }
                }


                if (Ss.Count == 0)
                {
                    // Return a 404 Not Found status with a JSON error message
                    var errorMessage = new { error = "No Subsations found" };
                    return Content(HttpStatusCode.NotFound, errorMessage);
                }

                return Ok(Ss); // Return the data as JSON
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
        }
    public class Ss
    {
        public string SsCode { get; set; }
        public string SsName { get; set; }
    }

}
