namespace AuctionApplication.Shared;

public class Bid : BaseEntity
{
    public User Bidder { get; set; } = new();
    public DateTime Time { get; set; }
    public decimal Value { get; set; }
    
    public Auction Auction { get; set; } = new();
}