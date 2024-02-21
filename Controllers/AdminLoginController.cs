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
    public class AdminLoginController : Controller
    {
        public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + "");
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(string useremail, string password)
        {
            try
            {
                if (!String.IsNullOrEmpty(useremail) && !String.IsNullOrEmpty(password))
                {
                    int result = 0;
                    SqlCommand sqlcmd = new SqlCommand("sp_admin", con);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@email", useremail.ToString());
                    sqlcmd.Parameters.AddWithValue("@password", password.ToString());
                    sqlcmd.Parameters.AddWithValue("@usertype", "admin");
                    SqlParameter onlogin = new SqlParameter();
                    onlogin.ParameterName = "@isValid";
                    onlogin.SqlDbType = SqlDbType.Bit;
                    onlogin.Direction = ParameterDirection.Output;
                    sqlcmd.Parameters.Add(onlogin);
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    sqlcmd.ExecuteNonQuery();
                    result = Convert.ToInt32(onlogin.Value);
                    if (result == 1)
                    {
                        Session["auser"] = useremail.ToString();
                        return Redirect("~/Admin");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "UserEmail or Password is wrong...";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Please Enter Login Details.";
                }

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return View();
        }
    }
}