using System.ComponentModel.DataAnnotations;

namespace Server.Models.Boards
{
    public class Boards : BaseModel
    {
        public string Name { get; set; } = string.Empty; // example: "White" or "Laracina"
        public string Type { get; set; } = string.Empty; // example: "MDF" or "Chip"
        public string Range { get; set; } = string.Empty; // example: "Classic" or "Prestige" (Grade or Pricing)
        public string Finish { get; set; } = string.Empty; // example: "Glossy" or "Matte"
        public int Thickness { get; set; } = 0;
        public int Length { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Quantity { get; set; } = 0;
        public decimal PricePerUnit { get; set; } = 0;
        public decimal TotalPrice { get; set; } = 0;

    }
}