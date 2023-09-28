namespace AuctionApplication.Shared;

public class Auction
{
    public int Id { get; set; }

    public string NameOfProduct { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime EndInclusive { get; set; }
    public User Owner { get; set; }
    
}