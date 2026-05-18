using System;

namespace LabModule.Models
{
    public class Recipe
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public int version { get; set; }
        public string status { get; set; }
    }
}