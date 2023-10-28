namespace AuctionApplication.Shared;

public class User : BaseEntity
{
    public string Nickname { get; set; } = string.Empty;
    
    public string Auth0Id { get; set; } = string.Empty;
    public ICollection<Auction> Auctions { get; set; }
}