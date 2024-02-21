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
    public class ProductController : Controller
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
        public ActionResult Manage_Product()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Manage_Product(string pname, HttpPostedFileBase pimage, int catId, int subCatId, string model, int bId, string description, string shortDesc, string uses, string addInfo, int proPrice, int proDisPrice, int isPromo, int isFeatured, int isTranding, int isDiscounted)
        {
            try
            {
                string _originalFileName = Path.GetFileName(pimage.FileName).ToLower();
                string _FileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                string _GetExtension = Path.GetExtension(pimage.FileName).ToLower();
                if (!String.IsNullOrEmpty(pname) && !String.IsNullOrEmpty(_FileName) && !String.IsNullOrEmpty(model) && !String.IsNullOrEmpty(description) && !String.IsNullOrEmpty(shortDesc) && !String.IsNullOrEmpty(uses) && !String.IsNullOrEmpty(addInfo))
                {

                    if (pimage.ContentLength > 0)
                    {
                        string _path = Path.Combine(Server.MapPath("~/Upload/Product"), _FileName);


                        if (con.State == ConnectionState.Closed)
                            con.Open();
                        SqlCommand sqlCommand = new SqlCommand("sp_Product", con);
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@proName", pname);
                        sqlCommand.Parameters.AddWithValue("@proImage", _FileName);
                        sqlCommand.Parameters.AddWithValue("@catId", catId);
                        sqlCommand.Parameters.AddWithValue("@subcatId", subCatId);
                        sqlCommand.Parameters.AddWithValue("@model", model.Trim());
                        sqlCommand.Parameters.AddWithValue("@bId", bId);
                        sqlCommand.Parameters.AddWithValue("@description", description);
                        sqlCommand.Parameters.AddWithValue("@shortDesc", shortDesc);
                        sqlCommand.Parameters.AddWithValue("@uses", uses);
                        sqlCommand.Parameters.AddWithValue("@addInfo", addInfo);
                        sqlCommand.Parameters.AddWithValue("@proPrice", proPrice);
                        sqlCommand.Parameters.AddWithValue("@proDisPrice", proDisPrice);
                        sqlCommand.Parameters.AddWithValue("@isPromo", isPromo);
                        sqlCommand.Parameters.AddWithValue("@isFeatured", isFeatured);
                        sqlCommand.Parameters.AddWithValue("@isTranding", isTranding);
                        sqlCommand.Parameters.AddWithValue("@isDiscounted", isDiscounted);
                        sqlCommand.Parameters.AddWithValue("@isDel", 0);
                        sqlCommand.Parameters.AddWithValue("@addDateTime", DateTime.Now);
                        sqlCommand.Parameters.AddWithValue("@Action", "I");


                        int status = sqlCommand.ExecuteNonQuery();
                        if (status == 1)
                        {
                            TempData["Message"] = "Added Successfully.......";
                            pimage.SaveAs(_path);
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
            return Redirect("~/Product");
        }

        [HttpGet]
        public ActionResult Update_Manage_Product(int proid, int cid, int csid, int bid)
        {

            List<tblCategory> categories = db.tblCategories.Where(x => x.isDel == false).ToList();
            ViewBag.categories = new SelectList(categories, "catId", "catName");
            ViewBag.proid = proid;
            ViewBag.cid = cid;
            ViewBag.csid = csid;
            ViewBag.bid = bid;
            return View();

        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Update_Manage_Product(int proid, string pname, HttpPostedFileBase pimage, int cid, int csid, string model, int bId, string pdesc, string shortDesc, string uses, string addInfo, decimal pprice, decimal pdisprice, int isPromo, int isFeatured, int isTranding, int isDiscounted, string imageUrl)
        {
            try
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlCommand sqlCommand = new SqlCommand("sp_Product", con);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@proId", proid);

                if (pimage != null)
                {
                    string _originalFileName = Path.GetFileName(pimage.FileName).ToLower();
                    string _FileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                    string _GetExtension = Path.GetExtension(pimage.FileName).ToLower();

                    if (pimage.ContentLength > 0)
                    {
                        string _path = Path.Combine(Server.MapPath("~/Upload/Product"), _FileName);
                        if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                        {
                            System.IO.File.Delete(Server.MapPath(imageUrl));
                        }
                        pimage.SaveAs(_path);
                    }
                    sqlCommand.Parameters.AddWithValue("@proImage", _FileName);

                }
                sqlCommand.Parameters.AddWithValue("@proName", pname.Trim());
                sqlCommand.Parameters.AddWithValue("@catId", cid);
                sqlCommand.Parameters.AddWithValue("@subcatId", csid);
                sqlCommand.Parameters.AddWithValue("@model", model.Trim());
                sqlCommand.Parameters.AddWithValue("@bId", bId);
                sqlCommand.Parameters.AddWithValue("@description", pdesc.Trim());
                sqlCommand.Parameters.AddWithValue("@shortDesc", shortDesc.Trim());
                sqlCommand.Parameters.AddWithValue("@uses", uses.Trim());
                sqlCommand.Parameters.AddWithValue("@addInfo", addInfo.Trim());
                sqlCommand.Parameters.AddWithValue("@proPrice", pprice);
                sqlCommand.Parameters.AddWithValue("@proDisPrice", pdisprice);
                sqlCommand.Parameters.AddWithValue("@isPromo", isPromo);
                sqlCommand.Parameters.AddWithValue("@isFeatured", isFeatured);
                sqlCommand.Parameters.AddWithValue("@isTranding", isTranding);
                sqlCommand.Parameters.AddWithValue("@isDiscounted", isDiscounted);
                sqlCommand.Parameters.AddWithValue("@Action", "U");
                int status = sqlCommand.ExecuteNonQuery();
                if (status == 1)
                {
                    ViewBag.Message = "Updated Successfully.......";
                }
                else
                {
                    ViewBag.Message = "Not Updated....";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Product");
        }

        [HttpGet]
        public ActionResult Delete_Product(int id, string imageUrl)
        {
            try
            {
                if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                {
                    System.IO.File.Delete(Server.MapPath(imageUrl));
                }
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlCommand sqlCommand = new SqlCommand("sp_Product", con);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@proId", Convert.ToInt32(id));
                sqlCommand.Parameters.AddWithValue("@isDel", "true");
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
            return Redirect("~/Product");
        }
        [HttpGet]
        public ActionResult ProImages(int id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProImages(int proId, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string _originalFileName = Path.GetFileName(file.FileName);
                        string _uniqueFileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                        string _path = Path.Combine(Server.MapPath("~/Upload/Product/ProductImages"), _uniqueFileName);
                        file.SaveAs(_path);

                        SqlCommand sqlCommand = new SqlCommand("sp_ProImages", con);
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@productId", proId);
                        sqlCommand.Parameters.AddWithValue("@images", _uniqueFileName);
                        sqlCommand.Parameters.AddWithValue("@Action", "I");
                        using (sqlCommand)
                        {
                            if (con.State == ConnectionState.Closed)
                                con.Open();

                            int status = sqlCommand.ExecuteNonQuery();

                            if (status == 1)
                                TempData["Message"] = "Added Successfully.......";
                            else
                                TempData["Message"] = "Not Added....";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Product");
        }

        [HttpGet]
        public ActionResult TrashProImages(int id, string imageUrl)
        {
            try
            {
                if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                {
                    System.IO.File.Delete(Server.MapPath(imageUrl));
                }
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlCommand sqlCommand = new SqlCommand("sp_ProImages", con);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@id", Convert.ToInt32(id));
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
            return Redirect("~/Product");
        }

        [HttpGet]
        public ActionResult Product_Attr(int id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public ActionResult Product_Attr(int proId, IEnumerable<HttpPostedFileBase> files, int colorId, int qty, string size)
        {
            try
            {
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string _originalFileName = Path.GetFileName(file.FileName);
                        string _uniqueFileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                        string _path = Path.Combine(Server.MapPath("~/Upload/Product/ProductAttrImages"), _uniqueFileName);
                        file.SaveAs(_path);

                        SqlCommand sqlCommand = new SqlCommand("sp_ProductAttr", con);
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@proId", proId);
                        sqlCommand.Parameters.AddWithValue("@proAttrImage", _uniqueFileName);
                        sqlCommand.Parameters.AddWithValue("@colorId", colorId); sqlCommand.Parameters.AddWithValue("@qty", qty); sqlCommand.Parameters.AddWithValue("@size", size); sqlCommand.Parameters.AddWithValue("@addDateTime", DateTime.Now); sqlCommand.Parameters.AddWithValue("@isDel", 0);
                        sqlCommand.Parameters.AddWithValue("@Action", "I");
                        using (sqlCommand)
                        {
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
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return Redirect("~/Product");
        }
        [HttpGet]
        public ActionResult Delete_ProImages(int id, string imageUrl)
        {
            try
            {
                if (System.IO.File.Exists(Server.MapPath(imageUrl)))
                {
                    System.IO.File.Delete(Server.MapPath(imageUrl));
                }
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlCommand sqlCommand = new SqlCommand("sp_ProductImagesInsertUpdateDelete", con);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@proAttrId", Convert.ToInt32(id));
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
            return Redirect("~/Product");
        }

        public JsonResult GetSubCategoryList(int catid)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<tblSubCategory> subCategories = db.tblSubCategories.Where(x => x.catId == catid).ToList();
            return Json(subCategories, JsonRequestBehavior.AllowGet);
        }
    }
}