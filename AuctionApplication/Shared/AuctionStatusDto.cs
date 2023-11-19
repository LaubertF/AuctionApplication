using System.Text.Json.Serialization;

namespace AuctionApplication.Shared;

public class AuctionStatusDto
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }
    [JsonPropertyName("NameOfProduct")]
    public string NameOfProduct { get; set; }
    [JsonPropertyName("Category")]
    public AuctionCategory Category { get; set; }
    [JsonPropertyName("StartInclusive")]
    public DateTime StartInclusive { get; set; } 
    [JsonPropertyName("EndInclusive")]
    public DateTime EndInclusive { get; set; }
    [JsonPropertyName("StartingPrice")]
    public decimal StartingPrice { get; set; }
    [JsonPropertyName("OwnerName")]
    public User Owner { get; set; } = new();
    [JsonPropertyName("WinnerName")]
    public User? Winner { get; set; } = null;
    [JsonPropertyName("IsClosed")]
    public bool IsClosed { get; set; }
    [JsonPropertyName("ProductImages")]
    public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    [JsonPropertyName("State")]
    public AuctionState State { get; set; }
    
    
}