using System;

namespace LabModule.Models
{
    public class Batch
    {
        public int id { get; set; }
        public string batch_number { get; set; }
        public int order_id { get; set; }
        public int recipe_id { get; set; }
        public int tech_map_id { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string status { get; set; }
        public decimal? actual_quantity_kg { get; set; }
        // навигационные свойства (необязательны)
        public Recipe recipe { get; set; }
        public TechMap tech_map { get; set; }
    }
}