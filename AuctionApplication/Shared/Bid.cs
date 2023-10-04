namespace AuctionApplication.Shared;

public class Bid
{
    public string BidderName { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public decimal Value { get; set; }
}