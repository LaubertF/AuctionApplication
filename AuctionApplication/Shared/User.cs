namespace AuctionApplication.Shared;

public class User : BaseEntity
{
    public string Nickname { get; set; } = string.Empty;
    public List<Auction> Auctions { get; set; } = new();
}