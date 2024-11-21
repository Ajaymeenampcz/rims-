using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WebApplication1;

namespace WebApplication1.Controllers
{
    public class TaskController : ApiController
    {
        //private readonly YourDbContext _context;

        //public TaskController()
        //{
        //    // Initialize your DbContext here
        //    _context = new YourDbContext(); // Replace YourDbContext with your actual context class
        //}

        //public TaskController()
        //{
        //    // Initialize your DbContext here
        //    var optionsBuilder = new DbContextOptionsBuilder<YourDbContext>();
        //    optionsBuilder.UseSqlServer("Server = 172.24.200.187; Database = ss_reno; User = sa; Password = sa@cz$123#;"); // Replace with your actual connection string
        //    _context = new YourDbContext(optionsBuilder.Options); // Replace YourDbContext with your actual context class
        //}



        [HttpPost]
        public async Task<IHttpActionResult> CreateTask()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Invalid request format");
            }

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                if (provider.Contents.Count != 24) // Check if all expected fields are present
                {
                    return BadRequest("Expected fields are missing or count mismatch.");
                }
                // Extract form fields
                var formFields = new Dictionary<string, string>();
                foreach (var content in provider.Contents)
                {
                    var fieldName = content.Headers.ContentDisposition.Name.Trim('"');
                    var fieldValue = await content.ReadAsStringAsync();
                    formFields[fieldName] = fieldValue;
                }

                // Access the uploaded files
                Stream job1_image1Stream = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "job1_image1Path").ReadAsStreamAsync().Result;
                Stream job1_image2Stream = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "job1_image2Path").ReadAsStreamAsync().Result;
                Stream job2_image1Stream = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "job2_image1Path").ReadAsStreamAsync().Result;
                Stream job2_image2Stream = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "job2_image2Path").ReadAsStreamAsync().Result;
                Stream job3_image1Stream = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "job3_image1Path").ReadAsStreamAsync().Result;
                Stream job3_image2Stream = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "job3_image2Path").ReadAsStreamAsync().Result;
                Stream job4_image1Stream = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "job4_image1Path").ReadAsStreamAsync().Result;
                Stream job4_image2Stream = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "job4_image2Path").ReadAsStreamAsync().Result;




                // Extract other form fields (e.g., region_code, circle_code, etc.)

                var job1_Name = formFields["job1_Name"];
                var job1_Remark = formFields["job1_Remark"];
                var job2_Name = formFields["job2_Name"];
                var job2_Remark = formFields["job2_Remark"];
                var job3_Name = formFields["job3_Name"];
                var job3_Remark = formFields["job3_Remark"];
                var job4_Name = formFields["job4_Name"];
                var job4_Remark = formFields["job4_Remark"];

                var region_code = formFields["region_code"];
                var circle_code = formFields["circle_code"];
                var division_code = formFields["division_code"];
                var dc_code = formFields["dc_code"];
                var ss_code = formFields["ss_code"];
                var updatedtime = formFields["updatedtime"];
                var updatedby_empname = formFields["updatedby_empname"];
                var updatedby_empcode = formFields["updatedby_empcode"];
                // Extract other fields as needed

          
                Console.WriteLine("Input dateTime: " + updatedtime);
                if (DateTime.TryParseExact(updatedtime, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime taskDateTime))
                {
                    Console.WriteLine("Parsed DateTime: " + taskDateTime.ToString()); // Print parsed DateTime
                                                                                      // DateTime parsing successful, use taskDateTime
                }
                else
                {
                    Console.WriteLine("Parsing failed");
                    return BadRequest("Invalid DateTime format");
                }


                string conn_str = "";
                config config = new config();
                conn_str = config.conn_str;
                string str = "";
                string connectionString = "Server = 172.24.200.187; Database = ss_reno; User = sa; Password = sa@cz$123#;";
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = conn_str;

                conn.Open();

                Int64 new_complaint_id = 0;
                SqlCommand cmd1 = new SqlCommand("getCompId", conn);


                cmd1.CommandType = CommandType.StoredProcedure;

                cmd1.Parameters.Add("@machine", SqlDbType.VarChar);
                cmd1.Parameters["@machine"].Value = 1;
                //cmd1.Parameters["@machine"].Value = HttpContext.Current.Session.SessionID;
                // cmd.Parameters["@machine"].Value = Session.SessionID;
                SqlDataReader rdr = cmd1.ExecuteReader();
                while (rdr.Read())
                {
                    new_complaint_id = rdr.GetInt64(0);
                    if (new_complaint_id == 0)
                    {

                        return BadRequest("Server busy. Please try after some time.");
                    }
                }

                rdr.Close();

                String year = DateTime.Now.Year.ToString();
                int month = DateTime.Now.Month;
                String mon = "";
                if (month > 9)
                    mon = month.ToString();
                else
                    mon = "0" + month.ToString();


                int day = DateTime.Now.Day;
                String d = "";

                if (day > 9)
                    d = day.ToString();
                else
                    d = "0" + day.ToString();

                String ncid = "";
                if (new_complaint_id < 10)
                {
                    ncid = "0000" + new_complaint_id.ToString();
                }

                else if (new_complaint_id < 100)
                {
                    ncid = "000" + new_complaint_id.ToString();
                }

                else if (new_complaint_id < 1000)
                {
                    ncid = "00" + new_complaint_id.ToString();
                }

                else if (new_complaint_id < 10000)
                {
                    ncid = "0" + new_complaint_id.ToString();
                }

                else if (new_complaint_id < 100000)
                {
                    ncid = new_complaint_id.ToString();
                }

                String job_no = year + mon + d + ncid;


                string relativeFolderPath = "uploads"; // Specify the relative folder path
                string job1_image1FileName = job_no + "_job1_image1.jpg"; // Specify the desired file name
                string job1_image2FileName = job_no + "_job1_image2.jpg"; // Specify the desired file name

                string job2_image1FileName = job_no + "_job2_image1.jpg"; // Specify the desired file name
                string job2_image2FileName = job_no + "_job2_image2.jpg"; // Specify the desired file name

                string job3_image1FileName = job_no + "_job3_image1.jpg"; // Specify the desired file name
                string job3_image2FileName = job_no + "_job3_image2.jpg"; // Specify the desired file name

                string job4_image1FileName = job_no + "job4_image1.jpg";// Specify the desired file name
                string job4_image2FileName = job_no + "job4_image2.jpg"; // Specify the desired file name

                // Get the absolute path within your application's directory
                string absoluteFolderPath = HttpContext.Current.Server.MapPath("~/" + relativeFolderPath);

                // Combine the folder path with the file name
                string job1_image1Path = Path.Combine(absoluteFolderPath, job1_image1FileName);
                string job1_image2Path = Path.Combine(absoluteFolderPath, job1_image2FileName);

                string job2_image1Path = Path.Combine(absoluteFolderPath, job2_image1FileName);
                string job2_image2Path = Path.Combine(absoluteFolderPath, job2_image2FileName);

                string job3_image1Path = Path.Combine(absoluteFolderPath, job3_image1FileName);
                string job3_image2Path = Path.Combine(absoluteFolderPath, job3_image2FileName);


                string job4_image1Path = Path.Combine(absoluteFolderPath, job4_image1FileName);
                string job4_image2Path = Path.Combine(absoluteFolderPath, job4_image2FileName);


                // Ensure the folder exists, create it if necessary
                Directory.CreateDirectory(absoluteFolderPath);

                // Create a file stream to save the data to the specified path(job1)
                using (FileStream fileStream = File.Create(job1_image1Path))
                {
                    await job1_image1Stream.CopyToAsync(fileStream);
                }

             
                using (FileStream fileStream = File.Create(job1_image2Path))
                {
                    await job1_image2Stream.CopyToAsync(fileStream);
                }

                // Create a file stream to save the data to the specified path(job2)
                using (FileStream fileStream = File.Create(job2_image1Path))
                {
                    await job2_image1Stream.CopyToAsync(fileStream);
                }

                
                using (FileStream fileStream = File.Create(job2_image2Path))
                {
                    await job2_image2Stream.CopyToAsync(fileStream);
                }

                // Create a file stream to save the data to the specified path(job3)
                using (FileStream fileStream = File.Create(job3_image1Path))
                {
                    await job3_image1Stream.CopyToAsync(fileStream);
                }

           
                using (FileStream fileStream = File.Create(job3_image2Path))
                {
                    await job3_image2Stream.CopyToAsync(fileStream);
                }


                // Create a file stream to save the data to the specified path(job4)
                using (FileStream fileStream = File.Create(job4_image1Path))
                {
                    await job4_image1Stream.CopyToAsync(fileStream);
                }

       
                using (FileStream fileStream = File.Create(job4_image2Path))
                {
                    await job4_image2Stream.CopyToAsync(fileStream);
                }


                //if (job1_Name == null || remark == null || taskDateTime == null || serializedTasks == null || imagePath == null || imagePath1 == null)
                //{
                //    // Log or print the values of these variables for debugging
                //    Console.WriteLine($"taskNumber: {taskNumber}");
                //    Console.WriteLine($"remark: {remark}");
                //    Console.WriteLine($"taskDateTime: {taskDateTime}");
                //    Console.WriteLine($"serializedTasks: {serializedTasks}");
                //    Console.WriteLine($"imagePath: {imagePath}");
                //    Console.WriteLine($"imagePath1: {imagePath1}");

                //    // Return a meaningful error response
                //    return BadRequest("One or more required fields are null.");
                //}



                TaskDataRow taskData = new TaskDataRow
                {
                    //TaskNumber = int.Parse(taskNumber),
                    job1_Name = job1_Name,
                    job1_Remark = job1_Remark,
                    // Set the file paths for the images
                    job1_image1Path = job1_image1Path,
                    job1_image2Path = job1_image2Path,

                    job2_Name = job2_Name,
                    job2_Remark = job2_Remark,
                    // Set the file paths for the images
                    job2_image1Path = job2_image1Path,
                    job2_image2Path = job2_image2Path,

                    job3_Name = job3_Name,
                    job3_Remark = job3_Remark,
                    // Set the file paths for the images
                    job3_image1Path = job3_image1Path,
                    job3_image2Path = job3_image2Path,

                    job4_Name = job4_Name,
                    job4_Remark = job4_Remark,
                    // Set the file paths for the images
                    job4_image1Path = job4_image1Path,
                    job4_image2Path = job4_image2Path,

                    region_code = Int64.Parse(region_code),
                    circle_code = Int64.Parse(circle_code),
                    division_code = Int64.Parse(division_code),
                    dc_code = Int64.Parse(dc_code),
                    ss_code = Int64.Parse(ss_code),
                    updatedtime = taskDateTime,
                    updatedby_empname = updatedby_empname,
                    updatedby_empcode = Int64.Parse(updatedby_empcode)

                };

                // Save taskData to your database
                //_context.TaskData.Add(taskData);
                //await _context.SaveChangesAsync();
             


                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("INSERT INTO SS_JOBENTRY (job1_Name,job1_Remark,job1_image1Path,job1_image2Path,job2_Name,job2_Remark,job2_image1Path,job2_image2Path,job3_Name,job3_Remark,job3_image1Path,job3_image2Path,job4_Name,job4_Remark,job4_image1Path,job4_image2Path,region_code,circle_code,division_code,dc_code,ss_code,updatedtime,updatedby_empname,updatedby_empcode,job_no) VALUES (@job1_Name,@job1_Remark,@job1_image1Path,@job1_image2Path,@job2_Name,@job2_Remark,@job2_image1Path,@job2_image2Path,@job3_Name,@job3_Remark,@job3_image1Path,@job3_image2Path,@job4_Name,@job4_Remark,@job4_image1Path,@job4_image2Path,@region_code,@circle_code,@division_code,@dc_code,@ss_code,@updatedtime,@updatedby_empname,@updatedby_empcode,@job_no)", connection))
                    {
                        command.Parameters.AddWithValue("@job1_Name", taskData.job1_Name);
                        command.Parameters.AddWithValue("@job1_Remark", taskData.job1_Remark);
                        command.Parameters.AddWithValue("@job1_image1Path", taskData.job1_image1Path);
                        command.Parameters.AddWithValue("@job1_image2Path", taskData.job1_image2Path);
                        command.Parameters.AddWithValue("@job2_Name", taskData.job2_Name);
                        command.Parameters.AddWithValue("@job2_Remark", taskData.job2_Remark);
                        command.Parameters.AddWithValue("@job2_image1Path", taskData.job2_image1Path);
                        command.Parameters.AddWithValue("@job2_image2Path", taskData.job2_image2Path);
                        command.Parameters.AddWithValue("@job3_Name", taskData.job3_Name);
                        command.Parameters.AddWithValue("@job3_Remark", taskData.job3_Remark);
                        command.Parameters.AddWithValue("@job3_image1Path", taskData.job3_image1Path);
                        command.Parameters.AddWithValue("@job3_image2Path", taskData.job3_image2Path);
                        command.Parameters.AddWithValue("@job4_Name", taskData.job4_Name);
                        command.Parameters.AddWithValue("@job4_Remark", taskData.job4_Remark);
                        command.Parameters.AddWithValue("@job4_image1Path", taskData.job4_image1Path);
                        command.Parameters.AddWithValue("@job4_image2Path", taskData.job4_image2Path);
                        command.Parameters.AddWithValue("@region_code", taskData.region_code);
                        command.Parameters.AddWithValue("@circle_code", taskData.circle_code);
                        command.Parameters.AddWithValue("@division_code", taskData.division_code);
                        command.Parameters.AddWithValue("@dc_code", taskData.dc_code);
                        command.Parameters.AddWithValue("@ss_code", taskData.ss_code);
                        command.Parameters.AddWithValue("@updatedtime", taskData.updatedtime);
                        command.Parameters.AddWithValue("@updatedby_empname", taskData.updatedby_empname);
                        command.Parameters.AddWithValue("@updatedby_empcode", taskData.updatedby_empcode);
                        command.Parameters.AddWithValue("@job_no", job_no);
                        command.ExecuteNonQuery();
                    }
                }

                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    StatusMessage = "Success",
                    Message = "Job created successfully!!Your Job No is " + job_no + "",
                    JobNumber = job_no
                });
               // return Ok("Job created successfully!!Your Job No is "+job_no+"");
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
}
