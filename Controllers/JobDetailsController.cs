using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using System.Configuration;
using WebApplication1;

namespace WebApplication1.Controllers
{
    public class JobDetailsController : ApiController
    {
        public IHttpActionResult GetJobDetailsByJobNo(string job_no)
        {
            try
            {
                string RegionName = "";
                string CircleName = "";
                string DivisionName = "";
                string DcName = "";
                string SsName = "";
                List<JobDetails> jobDetailsList = new List<JobDetails>();

                // Define your SQL connection string
                string connectionString = "Server=172.24.200.187;Database=ss_reno;User=sa;Password=sa@cz$123#;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT * FROM SS_JOBENTRY WHERE job_no = @JobNo";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@JobNo", job_no);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string connectionString1 = ConfigurationManager.ConnectionStrings["constr1"].ToString();
                                string updatedtime = reader["updatedtime"].ToString();

                                if (DateTime.TryParse(updatedtime, out DateTime updatedTime))
                                {
                                    Console.WriteLine("Parsed DateTime: " + updatedtime.ToString());
                                }
                                else
                                {
                                    Console.WriteLine("Parsing failed");
                                    return BadRequest("Invalid DateTime format");
                                }

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

                                var job1Name = reader["job1_Name"].ToString();
                                var job2Name = reader["job2_Name"].ToString();
                                var job3Name = reader["job3_Name"].ToString();
                                var job4Name = reader["job4_Name"].ToString();

                                if (!string.IsNullOrEmpty(job1Name))
                                {
                                    JobDetails jobDetails = new JobDetails
                                    {
                                        job_Name = job1Name,
                                        job_Remark = reader["job1_Remark"].ToString(),
                                        job_image1Path = reader["job1_image1Path"].ToString(),
                                        job_image2Path = reader["job1_image2Path"].ToString(),
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
                                        updatedby_empname = reader["updatedby_empname"].ToString(),
                                        updatedtime = updatedTime,
                                        job_no = Convert.ToInt64(reader["job_no"].ToString()),
                                    };

                                    jobDetailsList.Add(jobDetails);
                                }

                                if (!string.IsNullOrEmpty(job2Name))
                                {
                                    JobDetails jobDetails = new JobDetails
                                    {
                                        job_Name = job2Name,
                                        job_Remark = reader["job2_Remark"].ToString(),
                                        job_image1Path = reader["job2_image1Path"].ToString(),
                                        job_image2Path = reader["job2_image2Path"].ToString(),
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
                                        updatedby_empname = reader["updatedby_empname"].ToString(),
                                        updatedtime = updatedTime,
                                        job_no = Convert.ToInt64(reader["job_no"].ToString()),
                                    };

                                    jobDetailsList.Add(jobDetails);
                                }

                                if (!string.IsNullOrEmpty(job3Name))
                                {
                                    JobDetails jobDetails = new JobDetails
                                    {
                                        job_Name = job3Name,
                                        job_Remark = reader["job3_Remark"].ToString(),
                                        job_image1Path = reader["job3_image1Path"].ToString(),
                                        job_image2Path = reader["job3_image2Path"].ToString(),
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
                                        updatedby_empname = reader["updatedby_empname"].ToString(),
                                        updatedtime = updatedTime,
                                        job_no = Convert.ToInt64(reader["job_no"].ToString()),
                                    };

                                    jobDetailsList.Add(jobDetails);
                                }

                                if (!string.IsNullOrEmpty(job4Name))
                                {
                                    JobDetails jobDetails = new JobDetails
                                    {
                                        job_Name = job4Name,
                                        job_Remark = reader["job4_Remark"].ToString(),
                                        job_image1Path = reader["job4_image1Path"].ToString(),
                                        job_image2Path = reader["job4_image2Path"].ToString(),
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
                                        updatedby_empname = reader["updatedby_empname"].ToString(),
                                        updatedtime = updatedTime,
                                        job_no = Convert.ToInt64(reader["job_no"].ToString()),
                                    };

                                    jobDetailsList.Add(jobDetails);
                                }
                            }
                        }
                    }
                }

                var result = jobDetailsList
    .Where(j => !string.IsNullOrEmpty(j.job_Name) && j.job_Name.ToLower() != "null")
    .Select(j => new

                    {
                        job_Name = j.job_Name,
                        job_Remark = j.job_Remark,
                        job_image1Path = j.job_image1Path,
                        job_image2Path = j.job_image2Path,
                        region_code = j.region_code,
                        region_name = j.region_name,
                        circle_code = j.circle_code,
                        circle_name = j.circle_name,
                        division_code = j.division_code,
                        division_name = j.division_name,
                        dc_code = j.dc_code,
                        dc_name = j.dc_name,
                        ss_code = j.ss_code,
                        ss_name = j.ss_name,
                        updatedby_empname = j.updatedby_empname,
                        updatedtime = j.updatedtime,
                        job_no = j.job_no
                    }).ToList();

                return Ok(result);
            }
            catch (SqlException sqlEx)
            {
                // Handle SQL exceptions
                return InternalServerError(sqlEx);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return InternalServerError(ex);
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
}
