namespace AuctionApplication.Shared;
using System.ComponentModel.DataAnnotations;
using System;

public class Bid : BaseEntity
{
    public User Bidder { get; set; } = new();
    public DateTime Time { get; set; }
    [Required]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "Bid value cannot be negative.")]
    public decimal Value { get; set; }
    
    public Auction Auction { get; set; } = new();
    
    public override string ToString()
    {
        return "Bidder: " + Bidder.Id + " " + Bidder.Name + ", Auction: " + Auction.Id + ", Value: " + Value;
    }
}
