using System.Threading.Tasks;
using CsBases.Fundamentos;

class Program
{
  static async Task Main()
  {
    var laptop = new Product("Laptop", 1200);
    // WriteLine(laptop.GetDescription());
    var soporte = new ServiceProduct("Soporte Técnico", 300, 30);
    // WriteLine(soporte.GetDescription());

    var product = new Product("Mouse Gamer", 300);
    var productDto = ProductAdapter.ToDto(product);
    // WriteLine($"{productDto.Name} - {productDto.Price} - Código: {productDto.Code}");

    // Inyección de Dependencias
    ILabelService labelService = new LabelService();
    var manager = new ProductManager(labelService);
    var monitor = new Product("Monitor", 100);
    var installation = new ServiceProduct("Instalación del monitor", 20, 30);
    // manager.PrintLabel(monitor);
    // manager.PrintLabel(installation);

    var FirstProduct = await new ProductRepository().GetProduct(1);
    AttributeProcessor.ApplyUpperCase(FirstProduct);
    WriteLine($"{FirstProduct.Name}-{FirstProduct.Price}");
  }
}