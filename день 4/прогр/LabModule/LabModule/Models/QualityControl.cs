using System;

namespace LabModule.Models
{
    public class QualityControl
    {
        public int id { get; set; }
        public int? batch_id { get; set; }
        public int? raw_material_batch_id { get; set; }
        public DateTime analysis_date { get; set; }
        public string sample_type { get; set; } // "raw_material" или "finished_product"
        public string parameter_name { get; set; }
        public decimal? measured_value { get; set; }
        public string standard_value { get; set; } // например "10;20" или ">=5"
        public string unit { get; set; }
        public string result { get; set; } // "pass" / "fail"
        public string decision { get; set; } // "approved" / "blocked"
        public int? analyst_id { get; set; }
        public string analyst_comment { get; set; }
        public User analyst { get; set; }
        public Batch batch { get; set; }
        public RawMaterialBatch raw_material_batch { get; set; }
    }
}