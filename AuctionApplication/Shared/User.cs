using Microsoft.VisualBasic;

namespace AuctionApplication.Shared;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public string Auth0Id { get; set; } = string.Empty;

    public bool IsAdmin { get; set; } = false;
}