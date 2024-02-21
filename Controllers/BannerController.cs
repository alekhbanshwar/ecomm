using ecomm.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ecomm.Controllers
{
    public class BannerController : Controller
    {
        public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + "");

        ecommEntities db = new ecommEntities();
        common GetCommon = new common();
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(string bannerTitle, HttpPostedFileBase bannerImage, string bannerSmallTitle, string url, string descr)
        {
            try
            {
                string _originalFileName = Path.GetFileName(bannerImage.FileName).ToLower();
                string _FileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                string _GetExtension = Path.GetExtension(bannerImage.FileName).ToLower();


                var _FileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" }; // Add more extensions as needed

                if (bannerImage.ContentLength > 0 && bannerImage != null)
                {
                    string _path = Path.Combine(Server.MapPath("~/Upload/Banner"), _FileName);


                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    SqlCommand sqlCommand = new SqlCommand("sp_Banner", con);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@bannerTitle", bannerTitle);
                    sqlCommand.Parameters.AddWithValue("@descr", descr);
                    sqlCommand.Parameters.AddWithValue("@bannerSmallTitle", bannerSmallTitle);
                    sqlCommand.Parameters.AddWithValue("@url", url.Trim());
                    sqlCommand.Parameters.AddWithValue("@bannerImage", _FileName);
                    sqlCommand.Parameters.AddWithValue("@addedOn", DateTime.Now);
                    sqlCommand.Parameters.AddWithValue("@Action", "I");


                    int status = sqlCommand.ExecuteNonQuery();
                    if (status == 1)
                    {
                        TempData["Message"] = "Added Successfully.......";
                        bannerImage.SaveAs(_path);
                    }
                    else
                    {
                        TempData["Message"] = "Not Added....";
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Banner");
        }

        [HttpGet]
        public ActionResult Update_Banner(int id)
        {
            DataTable dataTable = new DataTable();
            try
            {

                string query = "SELECT * FROM tblBanner where bannerId=@id";
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    con.Open();
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return View(dataTable);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Update_Banner(int id, string bannerTitle, HttpPostedFileBase bannerImage, string bannerSmallTitle, string url, string descr, string previousImagePath)
        {
            try
            {
                if (bannerImage == null)
                {
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    SqlCommand sqlCommand = new SqlCommand("sp_Banner", con);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@bannerId", id);
                    sqlCommand.Parameters.AddWithValue("@bannerTitle", bannerTitle);
                    sqlCommand.Parameters.AddWithValue("@descr", descr);
                    sqlCommand.Parameters.AddWithValue("@bannerSmallTitle", bannerSmallTitle);
                    sqlCommand.Parameters.AddWithValue("@url", url.Trim());
                    sqlCommand.Parameters.AddWithValue("@Action", "U");
                    int status = sqlCommand.ExecuteNonQuery();
                    if (status == 1)
                    {
                        TempData["Message"] = "Updated Successfully.......";

                    }
                    else
                    {
                        TempData["Message"] = "Not Added....";
                    }
                }
                else
                {
                    if (System.IO.File.Exists(Server.MapPath(previousImagePath)))
                    {
                        System.IO.File.Delete(Server.MapPath(previousImagePath));
                    }
                    string _originalFileName = Path.GetFileName(bannerImage.FileName).ToLower();
                    string _FileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                    string _GetExtension = Path.GetExtension(bannerImage.FileName).ToLower();


                    var _FileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" }; // Add more extensions as needed

                    if (bannerImage.ContentLength > 0 && bannerImage != null)
                    {
                        string _path = Path.Combine(Server.MapPath("~/Upload/Banner"), _FileName);


                        if (con.State == ConnectionState.Closed)
                            con.Open();
                        SqlCommand sqlCommand = new SqlCommand("sp_Banner", con);
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@bannerId", id);
                        sqlCommand.Parameters.AddWithValue("@bannerTitle", bannerTitle);
                        sqlCommand.Parameters.AddWithValue("@descr", descr);
                        sqlCommand.Parameters.AddWithValue("@bannerSmallTitle", bannerSmallTitle);
                        sqlCommand.Parameters.AddWithValue("@url", url.Trim());
                        sqlCommand.Parameters.AddWithValue("@bannerImage", _FileName);
                        sqlCommand.Parameters.AddWithValue("@Action", "U");
                        int status = sqlCommand.ExecuteNonQuery();
                        if (status == 1)
                        {
                            TempData["Message"] = "Updated Successfully.......";
                            bannerImage.SaveAs(_path);
                        }
                        else
                        {
                            TempData["Message"] = "Not Added....";
                        }
                    }
                }
            }



            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Banner");
        }
        [HttpGet]
        public ActionResult Delete_Banner(int id, string imageUrl)
        {
            try
            {
                if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                {
                    System.IO.File.Delete(Server.MapPath(imageUrl));
                }
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlCommand sqlCommand = new SqlCommand("sp_Banner", con);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@bannerId", Convert.ToInt32(id));
                sqlCommand.Parameters.AddWithValue("@Action", "D");
                int status = sqlCommand.ExecuteNonQuery();
                if (status == 1)
                {
                    TempData["Message"] = "Deleted Successfully.......";

                }
                else
                {
                    TempData["Message"] = "Deleted Added....";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Banner");
        }
    }
}