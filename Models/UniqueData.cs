using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ecomm.Models
{
    public class UniqueData
    {
        public string[] getUniqueSize(string[] inputSize)
        {

            var uniqueElements = inputSize.Distinct().ToArray();
            return uniqueElements;
        }
    }
}