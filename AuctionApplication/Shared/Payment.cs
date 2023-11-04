namespace AuctionApplication.Shared;

public class Payment : BaseEntity
{
    public Auction Auction { get; set; } = new();
    public User User { get; set; } = new();
    public decimal Amount { get; set; } = new();
    public PaymentState State { get; set; } = new();
}