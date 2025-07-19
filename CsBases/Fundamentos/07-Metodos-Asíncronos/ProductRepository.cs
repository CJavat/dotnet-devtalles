using CsBases.Fundamentos;

public class ProductRepository
{
  public async Task<Product> GetProduct(int id)
  {
    WriteLine("Buscando producto...");
    await Task.Delay(2000);
    return new Product("Producto simulado", 500);
  }
}