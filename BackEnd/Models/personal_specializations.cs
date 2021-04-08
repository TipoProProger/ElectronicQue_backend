using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd.Models
{
    public class personal_specializations
    { 
        public int personal_specialization_id { get; set; }
        public int personal_id { get; set; }
        public int specialization_id { get; set; }
        public int personal_specialization_code { get; set; }
    }
}