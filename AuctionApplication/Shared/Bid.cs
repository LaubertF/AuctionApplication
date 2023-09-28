namespace AuctionApplication.Shared;

public class Bid
{
    public string BidderName { get; set; }
    public DateTime Time { get; set; }
    public decimal Value { get; set; }
}