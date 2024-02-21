using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ecomm.Models
{
    public class LoginModel
    {
        public string UserEmail { get; set; }
        public string Password { get; set; }
    }

    public class LoginResult
    {
        public bool IsValid { get; set; }
        public int UserID { get; set; }
    }
}