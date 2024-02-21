using ecomm.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ecomm.Controllers
{
    public class SubCategoryController : Controller
    {
        public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + "");

        ecommEntities db = new ecommEntities();
        [HttpGet]
        public ActionResult Index()
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (con)
                {
                    con.Open();
                    using (SqlCommand _sql = new SqlCommand("sp_SubCategory", con))
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
        public ActionResult Manage_SubCategory()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Manage_SubCategory(string subcategory, string catid)
        {
            try
            {
                if (catid != null && subcategory != null)
                {
                    SqlCommand sqlCommand = new SqlCommand("sp_SubCategory", con);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@subCatName", subcategory);
                    sqlCommand.Parameters.AddWithValue("@catId", catid);
                    sqlCommand.Parameters.AddWithValue("@isDel", "false");
                    sqlCommand.Parameters.AddWithValue("@Action", "Insert");
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    int status = sqlCommand.ExecuteNonQuery();
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
                    TempData["Message"] = "Please Insert Data.....";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/SubCategory");
        }

        [HttpGet]
        public ActionResult Update_Manage_SubCategory(int subCatId, int catId)
        {
            DataTable dataTable = new DataTable();
            try
            {

                string query = "SELECT * FROM tblSubCategory LEFT JOIN tblCategory on tblCategory.catId=tblSubCategory.catId where tblSubCategory.subCatId=@subcatid and tblCategory.catId=@catid";
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    con.Open();
                    command.Parameters.AddWithValue("@subcatid", subCatId);
                    command.Parameters.AddWithValue("@catid", catId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    con.Close();
                    ViewBag.SubCatData = dataTable;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return View(dataTable);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Update_Manage_SubCategory(int subcatid, string subcategory, string catid)
        {
            try
            {
                if (subcatid.ToString() != null && subcategory != null && catid != null)
                {

                    SqlCommand sqlCommand = new SqlCommand("sp_SubCategory", con);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@subCatId", subcatid);
                    sqlCommand.Parameters.AddWithValue("@subCatName", subcategory);
                    sqlCommand.Parameters.AddWithValue("@catId", catid);
                    sqlCommand.Parameters.AddWithValue("@Action", "Update");
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    int status = sqlCommand.ExecuteNonQuery();
                    if (status.ToString() == "1")
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
                    TempData["Message"] = "Please Insert Data.......";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/SubCategory");
        }
        [HttpGet]
        public ActionResult Delete_SubCategory(int id)
        {
            try
            {
                if (id != 0)
                {
                    using (con)
                    {
                        con.Open(); SqlCommand _sqlCommand = new SqlCommand("sp_SubCategory", con);
                        _sqlCommand.CommandType = CommandType.StoredProcedure;
                        _sqlCommand.Parameters.AddWithValue("@subCatId", id);
                        _sqlCommand.Parameters.AddWithValue("@Action", "Delete");
                        int status = _sqlCommand.ExecuteNonQuery();
                        if (status == 1)
                        {
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
            return Redirect("~/SubCategory");
        }

        public ActionResult Status(int id, int status)
        {
            try
            {
                con.Open();
                SqlCommand _sql = new SqlCommand("sp_SubCategory", con); _sql.CommandType = CommandType.StoredProcedure;
                _sql.Parameters.AddWithValue("@subCatId", id);
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
            return Redirect("~/SubCategory");
        }
    }
}