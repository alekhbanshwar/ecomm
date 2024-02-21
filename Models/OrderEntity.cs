using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ecomm.Models
{
    public class OrderEntity
    {
        public int customersId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string paymentType { get; set; }

        public int pincode { get; set; }
        public string totalAmt { get; set; }
        [NotMapped]
        public string TransactionId { get; set; }
        [NotMapped]
        public string TransactionOrderId { get; set; }
    }
}