using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using System.Diagnostics;
using System.Web;

namespace WebApplication1.Controllers
{
    public class LoginController : ApiController
    {

        [HttpPost]
        [Route("api/login")]
        public IHttpActionResult Authenticate(LoginRequestModel model)
        {
            try
            {
                string region_name1 = "";
                string circle_name1 = "";
                string division_name1 = "";
                string subdivision_name1 = "";
                string dc_name1 = "";

                // Check if login and password are provided
                if (string.IsNullOrEmpty(model.login) || string.IsNullOrEmpty(model.password))
                {
                    return ReturnJsonError(HttpStatusCode.BadRequest, "Login ID and password are required.");
                }

                //// URL-encode the password
                //model.password = HttpUtility.UrlEncode(model.password);

                string connStr = "Server = 172.24.100.120; Database = cz_crm; User = kpi_user; Password = User@123#;";

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Query to retrieve user data based on login
                    string query = "SELECT * FROM master_user WHERE user_employee_code = '" + model.login + "'";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                       // command.Parameters.AddWithValue("@login", model.login);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (model.login.Trim() == reader["user_employee_code"].ToString() && model.password.Trim() == reader["password"].ToString())
                                {
                                    // Store user data in session variables or any other desired storage method

                                    // Store user data in session variables
                                    if (HttpContext.Current != null)
                                    {
                                       
                                    if (HttpContext.Current.Session != null)
                                      {

                                    HttpContext.Current.Session["user_employee_code"] = reader["user_employee_code"];
                                    HttpContext.Current.Session["user_full_name"] = reader["user_full_name"];
                                    HttpContext.Current.Session["designation_id"] = reader["designation_id"];
                                    HttpContext.Current.Session["desig_name"] = reader["desig_name"];
                                    HttpContext.Current.Session["mobile"] = reader["mobile"];
                                    HttpContext.Current.Session["region_code"] = reader["region_code"];
                                    HttpContext.Current.Session["circle_code"] = reader["circle_code"];
                                    HttpContext.Current.Session["division_code"] = reader["division_code"];
                                    HttpContext.Current.Session["subdivision_code"] = reader["subdivision_code"];
                                    HttpContext.Current.Session["dc_code"] = reader["dc_code"];
                                    HttpContext.Current.Session["company_code"] = reader["company_code"];
                                    HttpContext.Current.Session["access_level_code"] = reader["access_level_code"];
                                        }
                                       else
                                        {
                                            //Handle the case where HttpContext.Session is null.
                                            return ReturnJsonError(HttpStatusCode.InternalServerError, "HttpContext.Session is null.");
                                        }
                                    }
                                    else
                                    {
                                        // Handle the case where HttpContext.Current is null.
                                        return ReturnJsonError(HttpStatusCode.InternalServerError, "HttpContext.Current is null.");
                                    }

                                    SqlConnection conn1 = new SqlConnection();
                                    conn1.ConnectionString = connStr;

                                    conn1.Open();

                                    SqlCommand command1 = new SqlCommand("select distinct region_name from master_region where region_code=" + reader["region_code"].ToString(), conn1);
                                    Debug.WriteLine("select distinct region_name from master_region where region_code=" + reader["region_code"].ToString() + "");
                                    using (SqlDataReader reader1 = command1.ExecuteReader())
                                    {
                                        while (reader1.Read())
                                        {
                                            region_name1 = reader1["region_name"].ToString();
                                        }

                                        reader1.Close();
                                    }

                                    command1 = new SqlCommand("select distinct circle_name from master_circle where circle_code=" + reader["circle_code"].ToString(), conn1);
                                    Debug.WriteLine("select distinct circle_name from master_circle where circle_code=" + reader["circle_code"].ToString() + "");
                                    using (SqlDataReader reader1 = command1.ExecuteReader())
                                    {
                                        while (reader1.Read())
                                        {
                                            circle_name1 = reader1["circle_name"].ToString();
                                        }

                                        reader1.Close();
                                    }

                                    command1 = new SqlCommand("select distinct division_name from master_division where division_code=" + reader["division_code"].ToString(), conn1);
                                    Debug.WriteLine("select distinct division_name from master_division where division_code=" + reader["division_code"].ToString() + "");
                                    using (SqlDataReader reader1 = command1.ExecuteReader())
                                    {
                                        while (reader1.Read())
                                        {
                                            division_name1 = reader1["division_name"].ToString();
                                        }

                                        reader1.Close();
                                    }

                                    command1 = new SqlCommand("select distinct subdivision_name from master_subdivision where subdivision_code=" + reader["subdivision_code"].ToString(), conn1);
                                    Debug.WriteLine("select distinct subdivision_name from master_subdivision where subdivision_code=" + reader["subdivision_code"].ToString() + "");
                                    using (SqlDataReader reader1 = command1.ExecuteReader())
                                    {
                                        while (reader1.Read())
                                        {
                                            subdivision_name1 = reader1["subdivision_name"].ToString();
                                        }

                                        reader1.Close();
                                    }

                                    command1 = new SqlCommand("select distinct dc_name from master_dc where dc_code=" + reader["dc_code"].ToString(), conn1);
                                  //  Debug.WriteLine("select distinct dc_name from master_dc where dc_code=" + reader["dc_code"].ToString() + "");
                                    using (SqlDataReader reader1 = command1.ExecuteReader())
                                    {
                                        while (reader1.Read())
                                        {
                                            dc_name1 = reader1["dc_name"].ToString();
                                        }

                                        reader1.Close();
                                    }

                                    conn1.Close();


                                    // Create a UserInfo object to represent the user data
                                    var userInfo = new
                                    {
                                        user_employee_code = reader["user_employee_code"],
                                        user_full_name = reader["user_full_name"],
                                        designation_id = reader["designation_id"],
                                        desig_name = reader["desig_name"],
                                        mobile = reader["mobile"],
                                        region_code = reader["region_code"],
                                        region_name = region_name1,
                                        circle_code = reader["circle_code"],
                                        circle_name = circle_name1,
                                        division_code = reader["division_code"],
                                        division_name = division_name1,
                                        subdivision_code = reader["subdivision_code"],
                                        subdivision_name = subdivision_name1,
                                        dc_code = reader["dc_code"],
                                        dc_name = dc_name1,
                                        company_code = reader["company_code"],
                                        access_level_code = reader["access_level_code"]
                                    };

                                    // Serialize the UserInfo object to JSON
                                    return ReturnJsonResponse(userInfo);
                                    //return Content(HttpStatusCode.OK, userInfo, new JsonMediaTypeFormatter());

                                }
                                else
                                {
                                    return ReturnJsonError(HttpStatusCode.Unauthorized, "Invalid login ID or password.");
                                }
                            }
                            else
                            {
                                return ReturnJsonError(HttpStatusCode.NotFound, "User not found.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes (optional)
              //  Debug.WriteLine($"Exception: {ex.Message}");

                return ReturnJsonError(HttpStatusCode.InternalServerError, "An internal server error occurred while processing the request.");
            }
        }

        private IHttpActionResult ReturnJsonResponse(object data)
        {
            return Content(HttpStatusCode.OK, data, new JsonMediaTypeFormatter());
        }

        private IHttpActionResult ReturnJsonError(HttpStatusCode statusCode, string errorMessage)
        {
            var errorResponse = new { error = errorMessage };
            return Content(statusCode, errorResponse, new JsonMediaTypeFormatter());
        }
    }


    public class LoginRequestModel
    {
        public string login { get; set; }
        public string password { get; set; }
    }
}