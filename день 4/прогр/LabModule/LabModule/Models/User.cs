using System;

namespace LabModule.Models
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
        public bool is_active { get; set; }
        public DateTime? last_login { get; set; }
        public DateTime created_at { get; set; }
    }
}