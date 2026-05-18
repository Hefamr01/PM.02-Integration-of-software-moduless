using System;

namespace OperatorModule.Models
{
    public class BatchStep
    {
        public int id { get; set; }
        public int batch_id { get; set; }
        public int step_order { get; set; }
        public string step_name { get; set; }
        public decimal? planned_temp_c { get; set; }
        public decimal? actual_temp_c { get; set; }
        public int? planned_duration_min { get; set; }
        public int? actual_duration_min { get; set; }
        public decimal? planned_pressure_bar { get; set; }
        public decimal? actual_pressure_bar { get; set; }
        public int? started_by { get; set; }
        public int? completed_by { get; set; }
        public DateTime? started_at { get; set; }
        public DateTime? completed_at { get; set; }
        public bool deviation_flag { get; set; }
        public string operator_comment { get; set; }
    }
}