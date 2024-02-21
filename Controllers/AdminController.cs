using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ecomm.Models;
using System.Web.Script.Serialization;

namespace ecomm.Controllers
{
    public class AdminController : Controller
    {
        public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + "");

        ecommEntities db = new ecommEntities();
        common GetCommon = new common();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Brand()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Brand(string bname, HttpPostedFileBase file)
        {
            try
            {
                DateTime currentDateTime = DateTime.Now;
                string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd");

                string _originalFileName = Path.GetFileName(file.FileName).ToLower();
                string _FileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                string _GetExtension = Path.GetExtension(file.FileName).ToLower();
                if (!String.IsNullOrEmpty(bname) && !String.IsNullOrEmpty(_FileName))
                    if (file.ContentLength > 0)
                    {
                        string _path = Path.Combine(Server.MapPath("~/Upload/Brand"), _FileName);


                        if (con.State == ConnectionState.Closed)
                            con.Open();
                        SqlCommand sqlCommand = new SqlCommand("sp_Brand", con);
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@brName", bname);
                        sqlCommand.Parameters.AddWithValue("@brImage", _FileName);
                        sqlCommand.Parameters.AddWithValue("@addDate", formattedDateTime);
                        sqlCommand.Parameters.AddWithValue("@isDel", "false");
                        sqlCommand.Parameters.AddWithValue("@Action", "I");

                        int status = sqlCommand.ExecuteNonQuery();
                        con.Close();
                        if (status == 1)
                        {
                            file.SaveAs(_path);
                            TempData["Message"] = "Added Successfully.......";
                        }
                        else
                        {
                            TempData["Message"] = "Not Added....";
                            return View();
                        }

                    }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return View();
        }

        [HttpGet]
        public ActionResult DeleteBrand(int id, string imageUrl)
        {
            try
            {
                if (id != 0)
                {
                    using (con)
                    {
                        con.Open(); SqlCommand _sqlCommand = new SqlCommand("sp_Brand", con);
                        _sqlCommand.CommandType = CommandType.StoredProcedure;
                        _sqlCommand.Parameters.AddWithValue("@brId", id);
                        _sqlCommand.Parameters.AddWithValue("@Action", "Delete");
                        int status = _sqlCommand.ExecuteNonQuery();
                        if (status == 1)
                        {
                            if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                            {
                                System.IO.File.Delete(Server.MapPath(imageUrl));
                            }
                            TempData["Message"] = "Deleted Successfully.......";
                        }
                        else
                        {
                            TempData["Message"] = "Not Deleted....";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Admin/Brand");
        }
        [HttpGet]
        public ActionResult Color()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Color(string cname)
        {
            try
            {
                if (!String.IsNullOrEmpty(cname))
                {
                    string query = "INSERT INTO tblColor(colName,isDel) VALUES('" + cname.Trim() + "','0')";
                    SqlCommand sqlcmd = new SqlCommand(query, con);
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    int status = sqlcmd.ExecuteNonQuery();
                    if (status == 1)
                    {
                        TempData["Message"] = "Added Successfully.......";
                    }
                    else
                    {
                        TempData["Message"] = "Not Added....";
                    }
                }
                else
                {
                    TempData["Message"] = "Please Insert Data......";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return View();
        }
        [HttpGet]
        public ActionResult Order()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Users()
        {
            return View();
        }
        public ActionResult Logout()
        {

            Session.Abandon();
            Session.Clear();
            TempData["Message"] = "Logout Successfully......";
            return Redirect("~/AdminLogin/Index");
        }
       

        //GenerateUniqueFileName

    }


}