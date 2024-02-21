using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace ecomm.Models
{
    public class common
    {
        public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["myCon"] + "");


        public int GetUserTempId()
        {
            if (HttpContext.Current.Session["USER_TEMP_ID"] == null)
            {
                Random random = new Random();
                int rand = random.Next(111111111, 999999999);
                HttpContext.Current.Session["USER_TEMP_ID"] = rand;
                return rand;
            }
            else
            {
                return (int)HttpContext.Current.Session["USER_TEMP_ID"];
            }

        }
        public List<tblCart> GetAddToCartTotalItem()
        {
            List<tblCart> dataList = new List<tblCart>();

            int UserId = 0;
            string UserType = "";

            if (HttpContext.Current.Session["USER_LOGIN"] != null)
            {
                UserId = (int)HttpContext.Current.Session["USER_ID"];
                UserType = "Reg";
            }
            else
            {
                UserId = GetUserTempId();
                UserType = "Not-Reg";
            }
            //DateTime currentDateTime = DateTime.Now;
            //string formattedDateTime = currentDateTime.ToString("yyyy-MM-dd");
            string query = "SELECT tblCarts.id as cart_id,tblCarts.qty, tblProduct.proName, tblProduct.proImage, tblProAttr.size, tblColor.colName, tblProduct.proDisPrice, tblProduct.proId as pid, tblProAttr.proAttrId as attr_id ,tblProAttr.proAttrImage,tblColor.colId FROM tblCarts " +
                           "LEFT JOIN tblProduct ON tblProduct.proId = tblCarts.proId " +
                           "LEFT JOIN tblProAttr ON tblProAttr.proAttrId = tblCarts.proAttrId " +
                           "LEFT JOIN tblColor ON tblColor.colId = tblProAttr.colorId " +
                           "WHERE tblCarts.userId = @UserId AND userType = @UserType";

            using (SqlCommand sql = new SqlCommand(query, con))
            {
                sql.Parameters.AddWithValue("@UserId", UserId);
                sql.Parameters.AddWithValue("@UserType", UserType);

                con.Open();

                using (SqlDataReader reader = sql.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tblCart dataItem = new tblCart
                        {
                            id = Convert.ToInt32(reader["cart_id"]),
                            qty = Convert.ToInt32(reader["qty"]),
                            proName = reader["proName"].ToString(),
                            proImage = reader["proImage"].ToString(),
                            size = reader["size"].ToString(),
                            colName = reader["colName"].ToString(),
                            price = Convert.ToDecimal(reader["proDisPrice"]),
                            proId = Convert.ToInt32(reader["pid"]),
                            proAttrId = Convert.ToInt32(reader["attr_id"]),
                            proAttrImage = reader["proAttrImage"].ToString(),
                            color = reader["colId"].ToString()

                        };

                        dataList.Add(dataItem);
                    }
                }
            }

            return dataList;
        }
        public int DeleteCart(int cart_id)
        {
            try
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlCommand sqlCommand = new SqlCommand("sp_Cart", con);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@id", cart_id);
                sqlCommand.Parameters.AddWithValue("@Action", "D");
                int status = sqlCommand.ExecuteNonQuery();
                return status;
            }
            catch (Exception)
            {
                throw;
            }


        }
        public string GenerateUniqueFileName(string _originalFileName)
        {
            string _uniqueIdentifier = Guid.NewGuid().ToString("N");

            string _fileExtension = Path.GetExtension(_originalFileName);

            string _uniqueFileName = $"{_uniqueIdentifier}{_fileExtension}";

            return _uniqueFileName;
        }
        public LoginResult CheckLogin(LoginModel loginModel)
        {
            using (con)
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("sp_CheckLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    cmd.Parameters.AddWithValue("@UserEmail", loginModel.UserEmail);
                    cmd.Parameters.AddWithValue("@Password", loginModel.Password);

                    // Output parameters
                    cmd.Parameters.Add("@IsValid", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Direction = ParameterDirection.Output;

                    int status = cmd.ExecuteNonQuery();
                    con.Close();
                    // Retrieve the output values
                    bool isValid = (bool)cmd.Parameters["@IsValid"].Value;
                    int userID = (int)cmd.Parameters["@UserID"].Value;

                    return new LoginResult
                    {
                        IsValid = isValid,
                        UserID = userID
                    };
                }
            }
        }


    }

}

