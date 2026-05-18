namespace OperatorModule.Models
{
    public class ProductionOrder
    {
        public int id { get; set; }
        public string order_number { get; set; }
        public int recipe_id { get; set; }
        public decimal planned_quantity_kg { get; set; }
        public string status { get; set; }
    }
}