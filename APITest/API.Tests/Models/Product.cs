namespace API.Tests.Models;

public class Product
{
    public int Id { get; set; }
    public int Category_id { get; set; }
    public string Title { get; set; }
    public string Alias { get; set; }
    public string Content { get; set; }
    public double Price { get; set; }
    public double Old_price { get; set; }
    public int Status { get; set; }
    public string Keywords { get; set; }
    public string Description { get; set; }
    public int Hit { get; set; }
}