using System.Collections.Generic;

namespace TechModule.Models
{
    public class DashboardData
    {
        public int active_products { get; set; }
        public int active_recipes { get; set; }
        public int active_tech_maps { get; set; }
        public int orders_in_work { get; set; }
        public int batches_in_production { get; set; }
        public int batches_with_deviations { get; set; }
        public int batches_waiting_lab_decision { get; set; }
        public List<object> latest_events { get; set; }
        public List<string> latest_events_json => latest_events.Select(x => x.ToString()).ToList();
    }
}