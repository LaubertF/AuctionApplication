namespace AuctionApplication.Shared;

public class ProductImage : BaseEntity
{
    public string Base64 { get; set; } = string.Empty;
    public Auction Auction { get; set; } = new Auction();
}