namespace AuctionApplication.Shared;

public class Bid : BaseEntity
{
    public string BidderName { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public decimal Value { get; set; }
    
    public Auction Auction { get; set; } = new();
}