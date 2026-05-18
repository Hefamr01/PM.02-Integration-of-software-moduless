using System;
using System.Collections.Generic;
using System.Text;

    namespace TechModule.Models
    {
        public class User
        {
            public int id { get; set; }
            public string username { get; set; }
            public string full_name { get; set; }
            public int role_id { get; set; }
            public int? department_id { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
        }
    }
