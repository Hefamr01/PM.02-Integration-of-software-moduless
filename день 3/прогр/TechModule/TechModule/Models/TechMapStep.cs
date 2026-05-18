namespace TechModule.Models
{
    public class TechMapStep
    {
        public int id { get; set; }
        public int tech_map_id { get; set; }
        public int step_order { get; set; }
        public string step_name { get; set; }
        public string step_type { get; set; }
        public decimal? planned_temp_c { get; set; }
        public decimal? planned_pressure_bar { get; set; }
        public int? planned_duration_min { get; set; }
        public bool is_mandatory { get; set; }
        public string instruction { get; set; }
    }
}