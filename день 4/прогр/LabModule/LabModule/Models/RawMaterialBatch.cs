using System;

namespace LabModule.Models
{
    public class RawMaterialBatch
    {
        public int id { get; set; }
        public int raw_material_id { get; set; }
        public string batch_number { get; set; }
        public string supplier { get; set; }
        public DateTime received_date { get; set; }
        public decimal quantity { get; set; }
        public string unit { get; set; }
        public string status { get; set; } // pending, in_analysis, approved, blocked
        public RawMaterial raw_material { get; set; }
    }
}