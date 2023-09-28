namespace AuctionApplication.Shared;

public class User
{
    public int Id { get; set; }
    public string Nickname { get; set; }
    public List<Auction> Auctions { get; set; }
}