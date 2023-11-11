using Microsoft.VisualBasic;

namespace AuctionApplication.Shared;

public class User : BaseEntity
{
    public string Auth0Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}