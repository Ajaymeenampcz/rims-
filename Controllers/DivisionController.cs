using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Globalization;
using ss_renovation_system;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Net.Http.Headers;

namespace WebApplication1.Controllers
{
    public class DivisionController : ApiController
    {

        [HttpGet]
        [Route("api/division/{circleCode}")]
        public IHttpActionResult GetDivisionData(string CircleCode)
        {
            try
            {
                string query = "SELECT DIVISION_CODE,DIVISION_NAME FROM MASTER_DIVISION WHERE CIRCLE_CODE = @CircleCode";

                if (@CircleCode == "")
                {
                    // Return a 404 Not Found status with a JSON error message
                    var errorMessage = new { error = "Kindly enter Circle code" };
                    return Content(HttpStatusCode.BadRequest, errorMessage);
                }

                List<Division> Division = new List<Division>();

                string connectionString = ConfigurationManager.ConnectionStrings["constr1"].ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CircleCode", CircleCode);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Division division = new Division
                                {
                                    DivisionCode = reader["DIVISION_CODE"].ToString(),
                                    DivisionName = reader["DIVISION_NAME"].ToString()
                                };

                                Division.Add(division);
                            }
                        }
                    }
                }

                //if (Division.Count == 0)
                //{
                //    // Return a 404 Not Found status with a JSON error message
                //    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                //    {
                //        Content = new StringContent("{'error': 'No data found'}", Encoding.UTF8, "application/json"),
                //        ReasonPhrase = "No Data Found"
                //    };

                //    throw new HttpResponseException(response);
                //}

              
                if (Division.Count == 0)
                {
                    // Return a 404 Not Found status with a JSON error message
                    var errorMessage = new { error = "No Division found" };
                    return Content(HttpStatusCode.NotFound, errorMessage);
                }

 
                return Ok(Division); // Return the data as JSON
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


    public class Division
    {
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
    }

}
