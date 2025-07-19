namespace CsBases.Fundamentos;

public class Product : IProduct
{
  public int Id { get; set; }
  [UpperCase]
  public string? Name { get; set; }
  public decimal Price { get; set; }
  public bool isAvailable { get; set; }
  public DateTime CreatedAt { get; set; }
  public Guid UniqueCode { get; set; }

  public Product(string name, decimal price)
  {
    Name = name;
    Price = price;
    CreatedAt = DateTime.Now;
    UniqueCode = Guid.NewGuid();
  }

  public void ApplyDiscount(decimal percentage)
  {
    var discountAmount = Price * (percentage / 10);
    Price -= discountAmount;
  }

  public virtual string GetDescription()
  {
    return $"{Name} - {Price:C}";
  }
}

class ServiceProduct : Product
{
  public int DurationInDays { get; set; }

  public ServiceProduct(string name, decimal price, int duration) : base(name, price)
  {
    DurationInDays = duration;
  }

  public override string GetDescription()
  {
    return $"{base.GetDescription()} = Duración: {DurationInDays} días";
  }
}
