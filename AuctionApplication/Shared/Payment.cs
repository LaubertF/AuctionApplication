using System.Text.Json.Serialization.Metadata;

namespace AuctionApplication.Shared;

public class Payment : BaseEntity
{
    public Auction Auction { get; set; }
    
    public User User { get; set; }
    
    public Decimal Value { get; set; }

    public PaymentState State { get; set; } = PaymentState.New;
    
    public DateTime DateCreated { get; set; }

    public DateTime? DateRegisterd { get; set; } = null;
    
    public DateTime? DatePaid { get; set; }  = null;
}