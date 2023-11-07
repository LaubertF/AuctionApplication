namespace AuctionApplication.Shared;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class BidDto
{
    [JsonPropertyName("AuctionId")]
    public int AuctionId { get; set; }

    [JsonPropertyName("BidderName")]
    public string BidderName { get; set; }

    [JsonPropertyName("Value")]
    public decimal Value { get; set; }

    [JsonPropertyName("Time")]
    public DateTime Time { get; set; }
}