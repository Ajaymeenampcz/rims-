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
    public class G2ccomplaintController : ApiController
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

                if (provider.Contents.Count != 13) // Check if all expected fields are present
                {
                    return BadRequest("Expected fields are missing or count mismatch.");
                }

                // Access the uploaded PDF file

                // Access the uploaded PDF file
                var pdfFileContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name.Trim('"') == "filepath");
                if (pdfFileContent == null)
                {
                    return BadRequest("PDF file is missing from the request.");
                }

                var file_name = pdfFileContent.Headers.ContentDisposition.FileName.Trim('"');
                var fileExtension = Path.GetExtension(file_name);

                if (!fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Invalid file type. Only PDF files are allowed.");
                }

                // Read the PDF file content as a byte array
                byte[] pdfFileBytes = await pdfFileContent.ReadAsByteArrayAsync();

                if (pdfFileBytes == null || pdfFileBytes.Length == 0)
                {
                    return BadRequest("PDF file content is null or empty.");
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
                Stream filepath = provider.Contents.First(c => c.Headers.ContentDisposition.Name.Trim('"') == "filepath").ReadAsStreamAsync().Result;
                



                // Extract other form fields (e.g., region_code, circle_code, etc.)

                var applicant_detail = formFields["applicant_detail"];
                var applicant_mobile = formFields["applicant_mobile"];
                var complaint_type = formFields["complaint_type"];
                var complaint_subtype = formFields["complaint_subtype"];
                var complaint_subject = formFields["complaint_subject"];
                var complaint_detail = formFields["complaint_detail"];
                var senior_officer = formFields["senior_officer"];
                var marked_officer = formFields["marked_officer"];
                var tagged_officer = formFields["tagged_officer"];
                var source = formFields["source"];

           
                var updated_by = formFields["updated_by"];
                var updated_on = formFields["updated_on"];
              
                // Extract other fields as needed

          
                Console.WriteLine("Input dateTime: " + updated_on);
                if (DateTime.TryParseExact(updated_on, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime taskDateTime))
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

                //String year = DateTime.Now.Year.ToString();
                int month1 = DateTime.Now.Month;
                int year1 = DateTime.Now.Year;
                int yearMonth = year1 * 100 + month1;



                //String mon = "";
                //if (month > 9)
                //    mon = month.ToString();
                //else
                //    mon = "0" + month.ToString();


                //int day = DateTime.Now.Day;
                //String d = "";

                //if (day > 9)
                //    d = day.ToString();
                //else
                //    d = "0" + day.ToString();

                //String ncid = "";
                //if (new_complaint_id < 10)
                //{
                //    ncid = "0000" + new_complaint_id.ToString();
                //}

                //else if (new_complaint_id < 100)
                //{
                //    ncid = "000" + new_complaint_id.ToString();
                //}

                //else if (new_complaint_id < 1000)
                //{
                //    ncid = "00" + new_complaint_id.ToString();
                //}

                //else if (new_complaint_id < 10000)
                //{
                //    ncid = "0" + new_complaint_id.ToString();
                //}

                //else if (new_complaint_id < 100000)
                //{
                //    ncid = new_complaint_id.ToString();
                //}
                //String COMP = "G2C";

                //String complaint_no = COMP + year + mon + d + ncid;


                string year = DateTime.Now.Year.ToString();
                int month = DateTime.Now.Month;
                int day = DateTime.Now.Day;
                string mon = month > 9 ? month.ToString() : "0" + month.ToString();
                string d = day > 9 ? day.ToString() : "0" + day.ToString();

                string ncid = new_complaint_id.ToString("D5");
                string COMP = "G2C";
                string complaint_no = COMP + year + mon + d + ncid;


                string relativeFolderPath = "uploads"; // Specify the relative folder path
                string fileName = complaint_no + "_file.pdf"; // Specify the desired file name
               
                // Get the absolute path within your application's directory
                string absoluteFolderPath = HttpContext.Current.Server.MapPath("~/" + relativeFolderPath);

                // Combine the folder path with the file name
                string finalFilePath = Path.Combine(absoluteFolderPath, fileName);
  


                // Ensure the folder exists, create it if necessary
                Directory.CreateDirectory(absoluteFolderPath);

                // Create a file stream to save the data to the specified path(job1)
                //using (var fileStream = File.Create(final_filepath))
                //{
                //    await pdfFileStream.CopyToAsync(fileStream);
                //}
                File.WriteAllBytes(finalFilePath, pdfFileBytes);

              //  using (var fileStream = new FileStream(final_filepath, FileMode.Create, FileAccess.Write))
              //  {
              //      await pdfFileStream.CopyToAsync(fileStream);
              //  }




                Task1DataRow task1Data = new Task1DataRow
                {
                   
                    applicant_detail = applicant_detail,
                    source = source,
                    applicant_mobile = Int64.Parse(applicant_mobile),

                    complaint_type = complaint_type,
                    complaint_subtype = complaint_subtype,

                    complaint_subject = complaint_subject,
                    complaint_detail = complaint_detail,
                    // Set the file paths for the images
                    senior_officer = senior_officer,
                    marked_officer = marked_officer,
                    tagged_officer = tagged_officer,

                    filepath = finalFilePath,
                    updated_by = updated_by,
                    // Set the file paths for the images
                    updated_on = taskDateTime,
                    

                };

                // Save taskData to your database
                //_context.TaskData.Add(taskData);
                //await _context.SaveChangesAsync();

                string senioroffcr_no = "";

                string markoffcer_no = "";
                string tagoffcer_no = "";

                string connectionString1 = "Data Source=172.24.200.187;Initial Catalog=UTTARA;User id =sa;password=sa@cz$123#";
                using (var connection = new SqlConnection(connectionString1))
                {
                    connection.Open();

                    SqlCommand CMD = new SqlCommand("select login_number from master_login where CIRCLE_CODE=0 and region_code=" + senior_officer + "", connection);
                    using (SqlDataReader reader = CMD.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            senioroffcr_no = reader[0].ToString();
                        }


                        reader.Close();
                    }

                    CMD = new SqlCommand("select login_number from master_login where access_level_code=3 AND circle_code=" + marked_officer + "", connection);
                    using (SqlDataReader reader = CMD.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            markoffcer_no = reader[0].ToString();
                        }


                        reader.Close();
                    }

                    CMD = new SqlCommand("select login_number from master_login where access_level_code=2 AND division_code=" + tagged_officer + "", connection);
                    using (SqlDataReader reader = CMD.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            tagoffcer_no = reader[0].ToString();
                        }


                        reader.Close();
                    }


                    using (var command = new SqlCommand("INSERT INTO g2c_complaint_entry (entry_id,source,applicant_detail, applicant_mobile, complaint_type, complaint_subtype, complaint_subject, complaint_detail, senior_officer, marked_officer, filepath, updated_by, updated_on, complaint_no, status, category, followup1, followup2, ref1, ref2, followup_date, forward_to, forwarded_by, forward_remark, revert_remark, closed_by, closed_remark, tagged_to, added_by, added_on, yyyymm) VALUES (@entry_id,@source,@applicant_detail, @applicant_mobile, @complaint_type, @complaint_subtype, @complaint_subject, @complaint_detail, @senior_officer, @marked_officer, @filepath, @updated_by, @updated_on, @complaint_no, @status, @category, @followup1, @followup2, @ref1, @ref2, @followup_date, @forward_to, @forwarded_by, @forward_remark, @revert_remark, @closed_by, @closed_remark, @tagged_to, @added_by, @added_on, @yyyymm)", connection))
                    {
                        // Add parameters with proper syntax
                        command.Parameters.AddWithValue("@entry_id",new_complaint_id);
                        command.Parameters.AddWithValue("@source", task1Data.source);
                        command.Parameters.AddWithValue("@applicant_detail", task1Data.applicant_detail);
                        command.Parameters.AddWithValue("@applicant_mobile", task1Data.applicant_mobile);
                        command.Parameters.AddWithValue("@complaint_type", task1Data.complaint_type);
                        command.Parameters.AddWithValue("@complaint_subtype", task1Data.complaint_subtype);
                        command.Parameters.AddWithValue("@complaint_subject", task1Data.complaint_subject);
                        command.Parameters.AddWithValue("@complaint_detail", task1Data.complaint_detail);
                        command.Parameters.AddWithValue("@senior_officer", senioroffcr_no);
                        command.Parameters.AddWithValue("@marked_officer", markoffcer_no);
                        command.Parameters.AddWithValue("@filepath", task1Data.filepath);
                        command.Parameters.AddWithValue("@updated_by","");
                        command.Parameters.AddWithValue("@updated_on", taskDateTime);
                        command.Parameters.AddWithValue("@complaint_no", complaint_no);
                        command.Parameters.AddWithValue("@status", 1);
                        command.Parameters.AddWithValue("@category", 1);
                        command.Parameters.AddWithValue("@followup1","");
                        command.Parameters.AddWithValue("@followup2", "");
                        command.Parameters.AddWithValue("@ref1", "");
                        command.Parameters.AddWithValue("@ref2", "");
                        command.Parameters.AddWithValue("@followup_date", taskDateTime);
                        command.Parameters.AddWithValue("@forward_to", 0);
                        command.Parameters.AddWithValue("@forwarded_by", 0);
                        command.Parameters.AddWithValue("@forward_remark", 0);
                        command.Parameters.AddWithValue("@revert_remark", 0);
                        command.Parameters.AddWithValue("@closed_by", 0);
                        command.Parameters.AddWithValue("@closed_remark", 0);
                        command.Parameters.AddWithValue("@tagged_to", tagoffcer_no);
                        command.Parameters.AddWithValue("@added_by", task1Data.updated_by);
                        command.Parameters.AddWithValue("@added_on", taskDateTime);
                        command.Parameters.AddWithValue("@yyyymm", yearMonth);
                        command.ExecuteNonQuery();
                }

                    //using (var command = new SqlCommand("INSERT INTO g2c_entry_transaction (entry_id,applicant_detail, applicant_mobile, complaint_type, complaint_subtype, complaint_subject, complaint_detail, senior_officer, marked_officer, filepath, updated_by, updated_on, complaint_no, status, category, followup1, followup2, ref1, ref2, followup_date, forward_to, forwarded_by, forward_remark, revert_remark, closed_by, closed_remark, tagged_to, added_by, added_on, yyyymm) VALUES (@entry_id,@applicant_detail, @applicant_mobile, @complaint_type, @complaint_subtype, @complaint_subject, @complaint_detail, @senior_officer, @marked_officer, @filepath, @updated_by, @updated_on, @complaint_no, @status, @category, @followup1, @followup2, @ref1, @ref2, @followup_date, @forward_to, @forwarded_by, @forward_remark, @revert_remark, @closed_by, @closed_remark, @tagged_to, @added_by, @added_on, @yyyymm)", connection))
                    using (var command = new SqlCommand("INSERT INTO g2c_entry_transaction (entry_id,category,from_office,senior_officer,marked_officer,tagged_officer,status,complaint_no,transaction_id,mobile_no,updated_by,updated_on,remark,yyyymm) VALUES (@entry_id,@category,@from_office,@senior_officer,@marked_officer,@tagged_officer,@status,@complaint_no,@transaction_id,@mobile_no,@updated_by,@updated_on,@remark,@yyyymm)", connection))
                    {
                        string comp_from = "G2C Complaint";
                        string complaint_new = complaint_no.Replace("G2C", "");
                        string txn = complaint_new + "0001";
                        Int64 txn_id = Convert.ToInt64(txn);
                        // Add parameters with proper syntax
                        command.Parameters.AddWithValue("@entry_id", new_complaint_id);
                        command.Parameters.AddWithValue("@category", 1);
                        command.Parameters.AddWithValue("@from_office", comp_from);
                        command.Parameters.AddWithValue("@senior_officer", senioroffcr_no);
                        command.Parameters.AddWithValue("@marked_officer", markoffcer_no);
                        command.Parameters.AddWithValue("@tagged_officer", tagoffcer_no);
                        command.Parameters.AddWithValue("@status", 1);
                        command.Parameters.AddWithValue("@complaint_no", complaint_no);
                        command.Parameters.AddWithValue("@transaction_id", txn_id);
                        command.Parameters.AddWithValue("@mobile_no", task1Data.applicant_mobile);
                        command.Parameters.AddWithValue("@updated_by", task1Data.updated_by);
                        command.Parameters.AddWithValue("@updated_on", taskDateTime);
                        command.Parameters.AddWithValue("@remark", task1Data.complaint_detail);
                        command.Parameters.AddWithValue("@yyyymm", yearMonth);
                       
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
            }

                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    StatusMessage = "Success",
                    Message = "Complaint registered successfully!!Your Complaint No is " + complaint_no + "",
                    ComplaintNumber = complaint_no
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
