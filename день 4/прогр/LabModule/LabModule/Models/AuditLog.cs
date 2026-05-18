using System;

namespace LabModule.Models
{
    public class AuditLog
    {
        public int id { get; set; }
        public string table_name { get; set; }
        public int record_id { get; set; }
        public string action { get; set; }
        public string old_value { get; set; }
        public string new_value { get; set; }
        public int? changed_by { get; set; }
        public DateTime changed_at { get; set; }
    }
}