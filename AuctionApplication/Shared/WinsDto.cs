namespace AuctionApplication.Shared;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class WinsDto
{
    [JsonPropertyName("Id")]
    public int AuctionId { get; set; }
    
    [JsonPropertyName("NameOfProduct")]
    public String NameOfProduct { get; set; }
    
    [JsonPropertyName("Value")]
    public Decimal Value { get; set; }
    
    [JsonPropertyName("Status")]
    public PaymentState State { get; set; }
}

