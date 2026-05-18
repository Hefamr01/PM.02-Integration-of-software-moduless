using System;

namespace TechModule.Models
{
    public class TechMap
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public int version { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public int created_by { get; set; }
    }
}