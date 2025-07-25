using ApiEcommerce.Models;

public interface IProductRepository
{
  ICollection<Product> GetProducts();
  ICollection<Product> GetProductsForCategory(int categoryId);
  ICollection<Product> SearchProducts(string searchTerm);
  Product? GetProduct(int id);
  bool BuyProduct(string productName, int quantity);
  bool ProductExists(int id);
  bool ProductExists(string name);
  bool CreateProduct(Product product);
  bool UpdateProduct(Product product);
  bool DeleteProduct(Product product);
  bool Save();
}