using System.Text.Json.Serialization;

namespace AuctionApplication.Shared;

public class AuctionDto
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }
    
    [JsonPropertyName("Title")]
    public string Title { get; set; }
    
    [JsonPropertyName("Description")]
    public string Description { get; set; }
    
    [JsonPropertyName("Category")]
    public AuctionCategory Category { get; set; }
    [JsonPropertyName("StartInclusive")]
    public DateTime StartInclusive { get; set; } 
    [JsonPropertyName("EndInclusive")]
    public DateTime EndInclusive { get; set; }
    [JsonPropertyName("StartingPrice")]
    public decimal StartingPrice { get; set; }
    [JsonPropertyName("BuyoutPrice")]
    public decimal? BuyoutPrice { get; set; } = null;
    [JsonPropertyName("OwnerName")]
    public string OwnerName { get; set; }
    [JsonPropertyName("WinnerName")]
    public string WinnerName { get; set; }
    [JsonPropertyName("IsClosed")]
    public bool IsClosed { get; set; }
}