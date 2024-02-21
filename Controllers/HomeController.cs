using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using ecomm.Models;
using System.IO;
using static ecomm.Models.common;
using Razorpay.Api;
using System.Collections.Generic;

namespace ecomm.Controllers
{
    public class HomeController : Controller
    {
        public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + "");
        public common GetCommon = new common();
        ecommEntities ecommEntities = new ecommEntities();
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Registration(string regName, string regEmail, string regMobile, string regPwd, string regZip, string regCpwd, HttpPostedFileBase profile)
        {
            try
            {
                string _originalFileName = Path.GetFileName(profile.FileName).ToLower();
                string _FileName = GetCommon.GenerateUniqueFileName(_originalFileName);
                string _GetExtension = Path.GetExtension(profile.FileName).ToLower();

                if (this.IsCaptchaValid("captch is not valid"))
                {
                    string _path = Path.Combine(Server.MapPath("~/Upload/Profile"), _FileName);

                    var _FileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" }; // Add more extensions as needed

                    if (profile.ContentLength > 0 && profile != null)
                    {
                        if (_FileExtensions.Contains(_GetExtension))
                        {
                            if (con.State == ConnectionState.Closed)
                                con.Open();
                            SqlCommand sqlCommand = new SqlCommand("sp_userRegistration", con);
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.Parameters.AddWithValue("@UserName", regName);
                            sqlCommand.Parameters.AddWithValue("@UserEmail", regEmail); sqlCommand.Parameters.AddWithValue("@Profile", _FileName);
                            sqlCommand.Parameters.AddWithValue("@MobileNumber", regMobile);
                            sqlCommand.Parameters.AddWithValue("@Password", regCpwd);
                            sqlCommand.Parameters.AddWithValue("@City", "NULL");
                            sqlCommand.Parameters.AddWithValue("@State", "NULL");
                            sqlCommand.Parameters.AddWithValue("@ZipCode", Convert.ToInt32(regZip));
                            sqlCommand.Parameters.AddWithValue("@Address", "NULL");
                            sqlCommand.Parameters.AddWithValue("@UserType", "Reg");
                            sqlCommand.Parameters.AddWithValue("@isDel", "false");
                            sqlCommand.Parameters.AddWithValue("@addRegDate", DateTime.Now);
                            sqlCommand.Parameters.AddWithValue("@Action", "I");

                            int status = sqlCommand.ExecuteNonQuery();
                            con.Close();
                            if (status == 1)
                            {
                                profile.SaveAs(_path);
                                TempData["Message"] = "Registration Successfully.";

                            }
                            else
                            {
                                TempData["Message"] = "Your Registration Not Successfully.";
                            }
                        }
                        else
                        {
                            TempData["Message"] = "Invalid file format. Please upload a .jpg,.jpeg,.png,.gif image file.";
                        }
                    }
                }
                else
                {
                    TempData["Message"] = "captcha is not valid.";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + ""))
                    {
                        con.Open(); // Open the database connection

                        LoginResult result = GetCommon.CheckLogin(loginModel);

                        if (result.IsValid)
                        {
                            // Store user information in session
                            Session["USER_LOGIN"] = true;
                            Session["USER_EMAIL"] = loginModel.UserEmail;
                            Session["USER_ID"] = result.UserID;

                            // Successful login
                            string query = "UPDATE tblCarts SET userId = @newUserId, userType = 'Reg' WHERE userId = @userId AND userType = @userType";
                            using (SqlCommand sqlCommand = new SqlCommand(query, con))
                            {
                                sqlCommand.Parameters.AddWithValue("@newUserId", result.UserID);
                                sqlCommand.Parameters.AddWithValue("@userId", GetCommon.GetUserTempId());
                                sqlCommand.Parameters.AddWithValue("@userType", "Not-Reg");

                                int status = sqlCommand.ExecuteNonQuery();

                                // Handle the result or redirect as needed
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            // Failed login
                            // You can add a model error or handle it based on your requirements
                            ModelState.AddModelError("", "Invalid login attempt.");
                        }
                    }
                }

                // If the ModelState is not valid, return to the login page with validation errors
                return View(loginModel);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return View("Index");
        }
        [HttpGet]
        public ActionResult Products(string sub_cat_name, string sort, string filter_price_start, string filter_price_end, string color_filter)
        {
            string sort_txt = "";
            string colorFilterArr = "";

            var query = from pro in ecommEntities.tblProducts
                        join c in ecommEntities.tblCategories on pro.catId equals c.catId
                        join sc in ecommEntities.tblSubCategories on pro.subCatId equals sc.subCatId
                        join pa in ecommEntities.tblProAttrs on pro.proId equals pa.proId into paGroup
                        where pro.isDel == false
                        select new { tblProducts = pro, tblProAttrs = paGroup };

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "name":
                        query = query.OrderBy(pro => pro.tblProducts.proName);
                        sort_txt = "Product Name";
                        break;
                    case "date":
                        query = query.OrderBy(pro => pro.tblProducts.addDateTime);
                        sort_txt = "Date";
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(pro => pro.tblProducts.proPrice);
                        sort_txt = "Price - DESC";
                        break;
                    case "price_asc":
                        query = query.OrderBy(pro => pro.tblProducts.proPrice);
                        sort_txt = "Price - ASC";
                        break;

                    case "default":
                        query = query.OrderBy(pro => pro.tblProducts.proId);
                        sort_txt = "Default";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(filter_price_start) && !string.IsNullOrEmpty(filter_price_end))
            {
                decimal startPrice, endPrice;
                if (decimal.TryParse(filter_price_start, out startPrice) && decimal.TryParse(filter_price_end, out endPrice))
                {
                    query = query.Where(pro => pro.tblProducts.proPrice >= startPrice && pro.tblProducts.proPrice <= endPrice);
                }
            }

            if (!string.IsNullOrEmpty(color_filter))
            {
                int colorId;
                if (int.TryParse(color_filter, out colorId))
                {
                    query = query.Where(pro => pro.tblProAttrs.Any(pa => pa.colorId == colorId));
                }
            }

            if (!string.IsNullOrEmpty(sub_cat_name))
            {
                int subCatId;
                if (int.TryParse(sub_cat_name, out subCatId))
                {
                    query = query.Where(pro => pro.tblProducts.subCatId == subCatId);
                }
            }

            var distinctProducts = query.Select(pro => pro.tblProducts).Distinct().ToList();
            var productAttributes = distinctProducts.ToDictionary(
                pro => pro.proId,
                pro => query.Where(q => q.tblProducts.proId == pro.proId).SelectMany(q => q.tblProAttrs).ToList()
            );

            var colors = ecommEntities.tblColors.Where(c => c.isDel == false).ToList();

            List<CategoryViewModel> categories = ecommEntities.tblCategories.Where(cat => cat.isDel == false).OrderBy(cat => cat.catName).Select(cat => new CategoryViewModel
            {
                CatId = cat.catId,
                CatName = cat.catName,
                Subcategories = ecommEntities.tblSubCategories
                .Where(scat => scat.isDel == false && scat.catId == cat.catId)
                .OrderBy(scat => scat.subCatName)
                .Select(scat => new SubCategoryViewModel
                {
                    SubCatId = scat.subCatId,
                    SubCatName = scat.subCatName
                }).ToList()
            }).ToList();




            ViewBag.product = distinctProducts;
            ViewBag.product_attr = productAttributes;
            ViewBag.colors = colors;
            ViewBag.sort = sort;
            ViewBag.sort_txt = sort_txt;
            ViewBag.filter_price_start = filter_price_start;
            ViewBag.filter_price_end = filter_price_end;
            ViewBag.color_filter = color_filter;
            ViewBag.colorFilterArr = colorFilterArr;
            ViewBag.catSubcat = categories;


            return View();
        }
        [HttpGet]
        public ActionResult Product_Details(int proid)
        {
            ViewBag.proId = proid;
            return View();
        }
        [HttpGet]
        public ActionResult Cart()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddToCart(int pid, int price, int qty, int color_id, string size_id)
        {
            try
            {
                if (color_id != 0)
                {
                    int UserId = 0;
                    string UserType = "";
                    int proAttrId = 0;

                    //get userid and usertype
                    if (Session["USER_LOGIN"] != null)
                    {
                        UserId = (int)Session["USER_ID"];
                        UserType = "Reg";

                    }
                    else
                    {
                        UserId = GetCommon.GetUserTempId();
                        UserType = "Not-Reg";


                    }
                    DataTable dt = new DataTable();
                    //select product attr id
                    string queryAttr = "SELECT proAttrId FROM tblProAttr WHERE proId=@proId and colorId=@colorId and size=@size";
                    using (SqlCommand commandAttr = new SqlCommand(queryAttr, con))
                    {
                        con.Open();
                        commandAttr.Parameters.AddWithValue("@proId", pid);
                        commandAttr.Parameters.AddWithValue("@colorId", color_id);
                        commandAttr.Parameters.AddWithValue("@size", size_id);
                        using (SqlDataAdapter adapterAttt = new SqlDataAdapter(commandAttr))
                        {
                            adapterAttt.Fill(dt);
                        }
                        con.Close();
                        foreach (DataRow rowAttr in dt.Rows)
                        {
                            proAttrId = Convert.ToInt32(rowAttr["proAttrId"]);
                        }
                    }

                    //Insert data in the cart table
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    SqlCommand sqlCommand = new SqlCommand("sp_Cart", con);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@userId", UserId);
                    sqlCommand.Parameters.AddWithValue("@userType", UserType);
                    sqlCommand.Parameters.AddWithValue("@qty", qty);
                    sqlCommand.Parameters.AddWithValue("@proId", pid);
                    sqlCommand.Parameters.AddWithValue("@proAttrId", proAttrId);
                    sqlCommand.Parameters.AddWithValue("@addedOn", DateTime.Now);
                    sqlCommand.Parameters.AddWithValue("@Action", "I");
                    int status = sqlCommand.ExecuteNonQuery();
                    con.Close();
                    if (status == 1)
                        TempData["Message"] = "cart added successfully...... ";
                    else
                        TempData["Message"] = "not added....... ";
                }
                else
                {
                    TempData["Message"] = "Please Select Color...... ";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult DeleteCart(int cid)
        {
            try
            {
                int r = GetCommon.DeleteCart(cid);
                con.Close();
                if (r > 0)
                {
                    TempData["Message"] = "Cart Deleted Successfully.....";
                }

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return RedirectToAction("Cart");
        }
        [HttpGet]
        public ActionResult Logout()
        {

            Session.Abandon();
            Session.Clear();
            TempData["Message"] = "Logout Successfully......";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Checkout()
        {
            ViewData["data"] = null;
            DataTable registrationData = new DataTable();
            var CartItems = GetCommon.GetAddToCartTotalItem();
            if (CartItems != null && CartItems.Count > 0)
            {
                if (Session["USER_LOGIN"] != null)
                {
                    int uid = (int)Session["USER_ID"];

                    //select regId according session
                    string queryReg = "SELECT * FROM tblRegistration WHERE id=@UserId";
                    using (SqlCommand command = new SqlCommand(queryReg, con))
                    {
                        con.Open();
                        command.Parameters.AddWithValue("@UserId", uid);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(registrationData);
                        }
                        con.Close();


                    }


                }
                else
                {
                    var session_not_user = Session["Id"];
                    //select regId according session
                    string queryReg = "SELECT * FROM tblRegistration WHERE Id=@Id";
                    using (SqlCommand command = new SqlCommand(queryReg, con))
                    {
                        con.Open();
                        command.Parameters.AddWithValue("@id", Convert.ToInt32(session_not_user));
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(registrationData);
                        }
                        con.Close();

                    }

                    // Store the list in ViewData

                }
                ViewData["RegistrationData"] = registrationData;
                ViewBag.CartItems = CartItems;
                return View("checkout");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        public ActionResult PlaceOrder()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(OrderEntity _orderEntity)
        {
            try
            {
                var CartItems = GetCommon.GetAddToCartTotalItem();
                // INSERTING tblOrders DATA
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlCommand command = new SqlCommand("InsertOrUpdateOrder", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@customersId", _orderEntity.customersId);
                command.Parameters.AddWithValue("@name", _orderEntity.name);
                command.Parameters.AddWithValue("@email", _orderEntity.email);
                command.Parameters.AddWithValue("@mobile", _orderEntity.mobile);
                command.Parameters.AddWithValue("@address", _orderEntity.address.Trim());
                command.Parameters.AddWithValue("@city", _orderEntity.city.Trim());
                command.Parameters.AddWithValue("@state", _orderEntity.state.Trim());
                command.Parameters.AddWithValue("@pincode", Convert.ToInt32(_orderEntity.pincode));
                command.Parameters.AddWithValue("@orderStatus", 1);
                command.Parameters.AddWithValue("@paymentType", _orderEntity.paymentType.Trim());
                command.Parameters.AddWithValue("@paymentStatus", "Pending");
                command.Parameters.AddWithValue("@totalAmt", _orderEntity.totalAmt);
                command.Parameters.AddWithValue("@trackDetails", "NULL");
                command.Parameters.AddWithValue("@added_on", DateTime.Now);
                command.Parameters.AddWithValue("@isDel", true);
                command.Parameters.AddWithValue("@Action", "I");
                SqlParameter outputParameter = new SqlParameter("@InsertedId", SqlDbType.Int);
                outputParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(outputParameter);

                int orderstatus = command.ExecuteNonQuery();
                //INSERTED ORDER ID FETCH 
                int insertedOrderId = (int)outputParameter.Value;
                Session["insertedOrderId"] = insertedOrderId;
                con.Close();
                ViewBag.id = insertedOrderId;
                if (orderstatus == 1 && insertedOrderId > 0 && CartItems != null && CartItems.Count > 0)
                {
                    int cart_id = 0; int product_id = 0; int products_attr_id = 0; int price = 0; int qty = 0;
                    foreach (var item in CartItems)
                    {
                        cart_id = Convert.ToInt32(item.id);
                        product_id = Convert.ToInt32(item.proId);
                        products_attr_id = Convert.ToInt32(item.proAttrId);
                        price = Convert.ToInt32(item.price);
                        qty = Convert.ToInt32(item.qty);

                        //INSERT tblOrderDetails Data From tblCart
                        if (con.State == ConnectionState.Closed)
                            con.Open();
                        SqlCommand cmd = new SqlCommand("InsertOrUpdateOrderDetails", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@orders_id", insertedOrderId);
                        cmd.Parameters.AddWithValue("@product_id", product_id);
                        cmd.Parameters.AddWithValue("@products_attr_id", products_attr_id);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@Action", "I");
                        int orderDetailsStatus = cmd.ExecuteNonQuery();
                        con.Close();
                        // AFTER INSERTING CARTDATA IN ORDERDETAILS TABLE THEN REMOVE FROM CART TABLE 
                        if (orderDetailsStatus == 1)
                        {
                            GetCommon.DeleteCart(cart_id);
                        }
                    }


                    //Payment GateWay
                    if (_orderEntity.paymentType == "Gateway")
                    {
                        var key = ConfigurationManager.AppSettings["RazorPaykey"].ToString();
                        var secret = ConfigurationManager.AppSettings["RazorPaysecret"].ToString();

                        Random _random = new Random();
                        string TransitionId = _random.Next(0, 1000).ToString();

                        RazorpayClient client = new RazorpayClient(key, secret);

                        Dictionary<string, object> input = new Dictionary<string, object>();

                        input.Add("amount", Convert.ToDecimal(_orderEntity.totalAmt) * 100);
                        input.Add("currency", "INR");
                        input.Add("receipt", TransitionId);

                        Order order = client.Order.Create(input);
                        ViewBag.order = order["id"].ToString();
                        ViewBag.name = _orderEntity.name;
                        ViewBag.email = _orderEntity.email;
                        ViewBag.mobile = _orderEntity.mobile;
                        ViewBag.address = _orderEntity.address;
                        ViewBag.city = _orderEntity.city;
                        ViewBag.pincode = _orderEntity.pincode;
                        return View("Payment", _orderEntity);
                    }

                }
                else
                {
                    TempData["message"] = "Please try after sometime";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = ex.Message;
            }

            return View();
        }
        [HttpPost]
        public ActionResult Payment(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
        {
            try
            {
                Dictionary<string, string> _attributes = new Dictionary<string, string>();
                _attributes.Add("razorpay_payment_id", razorpay_payment_id);
                _attributes.Add("razorpay_order_id", razorpay_order_id);
                _attributes.Add("razorpay_signature", razorpay_signature);

                Utils.verifyPaymentLinkSignature(_attributes);

                OrderEntity _orderDetails = new OrderEntity();
                _orderDetails.TransactionId = razorpay_payment_id;
                _orderDetails.TransactionOrderId = razorpay_order_id;
                return View("PaymentSuccess", _orderDetails);
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View("PaymentFailure");
            }

        }
        [HttpGet]
        public ActionResult PaymentSuccess(OrderEntity _orderDetails)
        {
            using (con)
            {
                con.Open();
                string _sqlQuery = "UPDATE tblOrders SET paymentId=@paymentId where id=@OrderId";
                using (SqlCommand _sqlCommand = new SqlCommand(_sqlQuery, con))
                {
                    _sqlCommand.Parameters.AddWithValue("@paymentId", _orderDetails.TransactionId);
                    _sqlCommand.Parameters.AddWithValue("@OrderId", Session["insertedOrderId"]);
                    int orderUpdate = _sqlCommand.ExecuteNonQuery();
                    con.Close();
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult PaymentFailure(OrderEntity _orderDetails)
        {
            using (con)
            {
                con.Open();
                string _sqlQuery = "UPDATE tblOrders SET paymentId=@paymentId where id=@OrderId";
                using (SqlCommand _sqlCommand = new SqlCommand(_sqlQuery, con))
                {
                    _sqlCommand.Parameters.AddWithValue("@paymentId", _orderDetails.TransactionId);
                    _sqlCommand.Parameters.AddWithValue("@OrderId", Session["insertedOrderId"]);
                    int orderUpdate = _sqlCommand.ExecuteNonQuery();
                    con.Close();
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult Order()
        {
            DataTable dataTable = new DataTable();
            int UserId = 0;
            string UserType = "";


            try
            {
                //get userid and usertype
                if (Session["USER_LOGIN"] == null)
                {

                    UserId = Convert.ToInt32(GetCommon.GetUserTempId());
                    UserType = "Not-Reg";
                }
                else
                {
                    DataTable dt = new DataTable();
                    //select regId according session
                    string queryReg = "SELECT id,UserType FROM tblRegistration WHERE UserEmail=@UserEmail";
                    using (SqlCommand command = new SqlCommand(queryReg, con))
                    {
                        con.Open();
                        command.Parameters.AddWithValue("@UserEmail", Session["USER_EMAIL"]);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                        con.Close();
                        foreach (DataRow row in dt.Rows)
                        {
                            UserId = Convert.ToInt32(row["id"]);
                            UserType = row["UserType"].ToString();
                        }
                    }
                }

                using (SqlCommand command = new SqlCommand("InsertOrUpdateOrder", con))
                {
                    con.Open();
                    command.CommandType = CommandType.StoredProcedure;
                    // Set the parameters
                    command.Parameters.AddWithValue("@customersId", UserId);
                    command.Parameters.AddWithValue("@Action", "S"); // for select
                    command.Parameters.Add("@InsertedId", SqlDbType.Int).Direction = ParameterDirection.Output;



                    // Use SqlDataAdapter to fill the DataTable
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            ViewData["data"] = dataTable;
            return View();
        }
        [HttpGet]
        public ActionResult OrderDetails(int orderId)
        {
            DataTable dataTable = new DataTable();
            try
            {

                using (SqlCommand command = new SqlCommand("InsertOrUpdateOrderDetails", con))
                {
                    con.Open();
                    command.CommandType = CommandType.StoredProcedure;
                    // Set the parameters
                    command.Parameters.AddWithValue("@orders_id", orderId);
                    command.Parameters.AddWithValue("@Action", "S"); // for select



                    // Use SqlDataAdapter to fill the DataTable
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            ViewData["data"] = dataTable;
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DecrementQuantity(int itemId, int qty)
        {
            try
            {
                int q = qty;
                string item = "SELECT * FROM tblCarts where id='" + itemId + "'";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(item, con);
                DataTable dt = new DataTable();
                sda.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    int i = qty - 1;
                    q--;
                }
                if (q == 0)
                {
                    string updateQty = "Delete tblCarts where id='" + itemId + "'";
                    SqlCommand sqlCommand = new SqlCommand(updateQty, con);
                    int status = sqlCommand.ExecuteNonQuery();
                    if (status == 1)
                    {
                        TempData["Message"] = "Cart Deleted.";
                    }
                    else
                    {
                        TempData["Message"] = "Quantity Not Updated.";
                    }
                }
                else
                {
                    string updateQty = "UPDATE tblCarts SET qty='" + q + "' where id='" + itemId + "'";
                    SqlCommand sqlCommand = new SqlCommand(updateQty, con);
                    int status = sqlCommand.ExecuteNonQuery();
                    if (status == 1)
                    {
                        TempData["Message"] = "Quantity Updated.";
                    }
                    else
                    {
                        TempData["Message"] = "Quantity Not Updated.";
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return View("Cart");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult IncrementQuantity(int itemId, int qty)
        {
            try
            {
                int q = qty;
                string item = "SELECT * FROM tblCarts where id='" + itemId + "'";
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(item, con);
                DataTable dt = new DataTable();
                sda.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    int i = qty + 1;
                    q++;
                }

                if (q > 0)
                {
                    string updateQty = "UPDATE tblCarts SET qty='" + q + "' where id='" + itemId + "'";
                    SqlCommand sqlCommand = new SqlCommand(updateQty, con);
                    int status = sqlCommand.ExecuteNonQuery();
                    if (status == 1)
                    {
                        TempData["Message"] = "Quantity Updated.";
                    }
                    else
                    {
                        TempData["Message"] = "Quantity Not Updated.";
                    }
                }



            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return View("Cart");
        }
        [HttpGet]
        public ActionResult Search(string searchStr)
        {
            try
            {
                DataTable dt = new DataTable();
                DataTable dataTable = new DataTable();

                using (con)
                {
                    con.Open();

                    var _select = "SELECT * FROM tblProduct LEFT JOIN tblCategory ON tblProduct.catId=tblCategory.catId LEFT JOIN tblSubCategory ON tblProduct.subcatId=tblSubCategory.subCatId LEFT JOIN tblProAttr on tblProduct.proId=tblProAttr.proId LEFT JOIN tblbrand on tblProduct.bId=tblbrand.brId LEFT JOIN tblColor ON tblProAttr.colorId=tblColor.colId WHERE tblProduct.isDel='FALSE' AND ( tblProduct.proName LIKE '%' + @searchString + '%' OR tblProduct.model LIKE '%' + @searchString + '%' OR tblProduct.shortDesc LIKE '%' + @searchString + '%' OR tblProduct.description LIKE '%' + @searchString + '%' OR tblProduct.uses LIKE '%' + @searchString + '%' OR tblSubCategory.subCatName LIKE '%' + @searchString + '%' OR tblCategory.catName LIKE '%' + @searchString + '%' OR tblbrand.brName LIKE '%' + @searchString + '%') ORDER BY tblProduct.addDateTime";

                    using (SqlCommand _sqlCommand = new SqlCommand(_select, con))
                    {
                        _sqlCommand.Parameters.AddWithValue("@searchString", searchStr);

                        using (SqlDataAdapter _adapter = new SqlDataAdapter(_sqlCommand))
                        {
                            _adapter.Fill(dataTable);
                        }
                    }

                    int rowcount = dataTable.Rows.Count;

                    if (rowcount > 0)
                    {
                        foreach (DataRow dr in dataTable.Rows)
                        {
                            int id = (dr["proId"] != DBNull.Value) ? (int)dr["proId"] : 0;

                            using (SqlConnection con2 = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + ""))
                            {
                                con2.Open();

                                var query = "SELECT * FROM tblProAttr LEFT JOIN tblColor ON tblProAttr.colorId=tblColor.colId WHERE tblProAttr.proId=@id";

                                using (SqlCommand sql = new SqlCommand(query, con2))
                                {
                                    sql.Parameters.AddWithValue("@id", id);

                                    using (SqlDataAdapter _adapter = new SqlDataAdapter(sql))
                                    {
                                        _adapter.Fill(dt);
                                    }
                                }
                            }
                        }
                    }
                }

                ViewData["data"] = dt;
                ViewData["data"] = dataTable;
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return View();

        }

    }
}