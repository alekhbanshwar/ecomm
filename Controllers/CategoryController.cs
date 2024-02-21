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
    public class CategoryController : Controller
    {
        public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + "");

        ecommEntities db = new ecommEntities();
        common GetCommon = new common();
        [HttpGet]
        public ActionResult Index()
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (con)
                {
                    con.Open();
                    using (SqlCommand _sql = new SqlCommand("sp_Category", con))
                    {
                        _sql.CommandType = CommandType.StoredProcedure;
                        _sql.Parameters.AddWithValue("@Action", "Select");
                        using (SqlDataAdapter adapter = new SqlDataAdapter(_sql))
                        {
                            adapter.Fill(dataTable);
                        }
                        con.Close();
                        ViewData["data"] = dataTable;
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
        public ActionResult Manage_Category()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manage_Category(string category, HttpPostedFileBase file)
        {
            try
            {
                string _originalFileName = Path.GetFileName(file.FileName).ToLower();
                string _FileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                string _GetExtension = Path.GetExtension(file.FileName).ToLower();
                if (!String.IsNullOrEmpty(category) && !String.IsNullOrEmpty(_FileName))
                    if (file.ContentLength > 0)
                    {
                        string _path = Path.Combine(Server.MapPath("~/Upload/Category"), _FileName);

                        if (con.State == ConnectionState.Closed)
                            con.Open();
                        SqlCommand sqlCommand = new SqlCommand("sp_Category", con);
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@catName", category);
                        sqlCommand.Parameters.AddWithValue("@catImage", _FileName);
                        sqlCommand.Parameters.AddWithValue("@isDel", "false");
                        sqlCommand.Parameters.AddWithValue("@Action", "Insert");

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
                        }

                    }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Category");
        }

        [HttpGet]
        public ActionResult Update_Manage_Category(int id)
        {
            DataTable dataTable = new DataTable();
            try
            {

                string query = "SELECT * FROM tblCategory where catId=@id";
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
        public ActionResult Update_Manage_Category(int id, string category, HttpPostedFileBase file, string imageUrl)
        {
            try
            {
                if (file == null)
                {
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    SqlCommand _sqlCommand = new SqlCommand("sp_Category", con);
                    _sqlCommand.CommandType = CommandType.StoredProcedure;
                    _sqlCommand.Parameters.AddWithValue("@catId", id);
                    _sqlCommand.Parameters.AddWithValue("@catName", category.Trim());
                    _sqlCommand.Parameters.AddWithValue("@Action", "Update");
                    int status = _sqlCommand.ExecuteNonQuery();
                    if (status == 1)
                    {
                        TempData["Message"] = "Updated Successfully.......";
                    }
                    else
                    {
                        TempData["Message"] = "Not Updated....";
                    }
                }
                else
                {

                    string _originalFileName = Path.GetFileName(file.FileName).ToLower();
                    string _FileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                    string _GetExtension = Path.GetExtension(file.FileName).ToLower();
                    if (file.ContentLength > 0)
                    {
                        string _path = Path.Combine(Server.MapPath("~/Upload/Category"), _FileName);

                        con.Open();
                        SqlCommand _sqlCommand = new SqlCommand("sp_Category", con);
                        _sqlCommand.CommandType = CommandType.StoredProcedure;
                        _sqlCommand.Parameters.AddWithValue("@catId", id);
                        _sqlCommand.Parameters.AddWithValue("@catName", category.Trim());
                        _sqlCommand.Parameters.AddWithValue("@catImage", _FileName);
                        _sqlCommand.Parameters.AddWithValue("@Action", "Update");
                        int status = _sqlCommand.ExecuteNonQuery();
                        if (status == 1)
                        {
                            file.SaveAs(_path);
                            if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                            {
                                System.IO.File.Delete(Server.MapPath(imageUrl));
                            }
                            TempData["Message"] = "Updated Successfully.......";
                        }
                        else
                        {
                            TempData["Message"] = "Not Updated....";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Category");
        }
        [HttpGet]
        public ActionResult Delete_Category(int id, string imageUrl)
        {
            try
            {
                if (id != 0)
                {
                    using (con)
                    {
                        con.Open(); SqlCommand _sqlCommand = new SqlCommand("sp_Category", con);
                        _sqlCommand.CommandType = CommandType.StoredProcedure;
                        _sqlCommand.Parameters.AddWithValue("@catId", id);
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
            return Redirect("~/Category");
        }

        public ActionResult Status(int id, int status)
        {
            try
            {
                con.Open();
                SqlCommand _sql = new SqlCommand("sp_Category", con); _sql.CommandType = CommandType.StoredProcedure;
                _sql.Parameters.AddWithValue("@catId", id);
                if (status == 1)
                {

                    _sql.Parameters.AddWithValue("@isDel", 0);

                }
                else if (status == 0)
                {
                    _sql.Parameters.AddWithValue("@isDel", 1);
                }
                _sql.Parameters.AddWithValue("@Action", "ADtive");
                int s = _sql.ExecuteNonQuery();
                if (s == 1)
                {
                    TempData["Message"] = "Updated Successfully.......";
                }
                else
                {
                    TempData["Message"] = "Not Updated....";
                }

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Category");
        }
    }
}