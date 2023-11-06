namespace AuctionApplication.Shared;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class PaymentData
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }
    
    [JsonPropertyName("Value")]
    public Decimal Value { get; set; }
    
    [JsonPropertyName("NameOfProduct")]
    public String NameOfProduct { get; set; }
}