using System.ComponentModel.DataAnnotations;

namespace AuctionApplication.Shared;

public class Auction : BaseEntity
{
    
    [Required]
    [DataType(DataType.Text)]
    public string NameOfProduct { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Text)]
    public string Description { get; set; } = string.Empty;
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "Starting price cannot be negative.")]
    public decimal StartingPrice { get; set; }

    [Required(ErrorMessage = "End Date is required.")]
    [DataType(DataType.Date)]
    [DateGreaterThan("StartInclusive", ErrorMessage = "End Date must be greater than Start Date.")]
    public DateTime EndInclusive { get; set; } = DateTime.Now.AddDays(2);
    [Required(ErrorMessage = "Start Date is required.")]
    [DataType(DataType.Date)]
    [FutureDate(ErrorMessage = "The date must be in the future.")]
    public DateTime StartInclusive { get; set; } = DateTime.Now.AddDays(1);
    public User Owner { get; set; } = new();
    public User? Winner { get; set; } = null;
    public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public bool IsClosed { get; set; }
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "Buyout price cannot be negative.")]
    public decimal BuyoutPrice { get; set; }
    
    public AuctionCategory Category { get; set; } = AuctionCategory.Other;
}

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date > DateTime.Now)
            {
                return ValidationResult.Success;
            }
        }
        return new ValidationResult(ErrorMessage ?? "The date must be in the future.");
    }
}

public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateGreaterThanAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var startDateProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

        if (startDateProperty == null)
        {
            return new ValidationResult($"Unknown property: {_comparisonProperty}");
        }

        var startDateValue = (DateTime)startDateProperty.GetValue(validationContext.ObjectInstance);
        var endDateValue = (DateTime)value;

        if (endDateValue > startDateValue)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? "End Date must be greater than Start Date.");
    }
}