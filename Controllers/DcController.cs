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
    public class DcController : ApiController
    {

        [HttpGet]
        [Route("api/dc/{divisionCode}")]
        public IHttpActionResult GetDcData(string DivisionCode)
        {
            try
            {
                string query = "SELECT dc_code,dc_name FROM master_dc WHERE division_code = @DivisionCode";

                if (@DivisionCode == "")
                {
                    // Return a 404 Not Found status with a JSON error message
                    var errorMessage = new { error = "Kindly enter Division code" };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                List<Dc> Dc = new List<Dc>();

                string connectionString = ConfigurationManager.ConnectionStrings["constr1"].ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DivisionCode", DivisionCode);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dc dc = new Dc
                                {
                                    DcCode = reader["dc_code"].ToString(),
                                    DcName = reader["dc_name"].ToString()
                                };

                                Dc.Add(dc);
                            }
                        }
                    }
                }


                if (Dc.Count == 0)
                {
                    // Return a 404 Not Found status with a JSON error message
                    var errorMessage = new { error = "No Dc found" };
                    return Content(HttpStatusCode.NotFound, errorMessage);
                }

                return Ok(Dc); // Return the data as JSON
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
    public class Dc
    {
        public string DcCode { get; set; }
        public string DcName { get; set; }
    }

}
