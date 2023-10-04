namespace AuctionApplication.Shared;

public class User : BaseEntity
{
    public string Nickname { get; set; }
    public List<Auction> Auctions { get; set; }
}