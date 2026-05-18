namespace TechModule.Models
{
    public class RecipeComponent
    {
        public int id { get; set; }
        public int recipe_id { get; set; }
        public int raw_material_id { get; set; }
        public decimal percentage { get; set; }
        public int load_order { get; set; }
        public decimal tolerance_min { get; set; }
        public decimal tolerance_max { get; set; }
    }
}