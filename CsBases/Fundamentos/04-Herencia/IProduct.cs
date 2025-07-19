namespace CsBases.Fundamentos;

public interface IProduct
{
  void ApplyDiscount(decimal percentage);
  string GetDescription();
}
