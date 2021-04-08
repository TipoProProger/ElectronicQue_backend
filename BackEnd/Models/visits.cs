using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd.Models
{
    public class visits
    {
        public int visit_id { get; set; }
        public int citizen_id { get; set; }
        public DateTime visit_date { get; set; }
        public int personal_specialization_code {get; set;}
        public string visit_time { get; set; }
    }
}