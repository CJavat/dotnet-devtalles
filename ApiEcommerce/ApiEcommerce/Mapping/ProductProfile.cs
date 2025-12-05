using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Mapster;

namespace ApiEcommerce.Mapping;

public static class ProductMappingConfig
{
  public static void Register()
  {
    TypeAdapterConfig<Product, ProductDto>.NewConfig()
        .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null);
    TypeAdapterConfig<ProductDto, Product>.NewConfig();
    TypeAdapterConfig<Product, CreateProductDto>.NewConfig();
    TypeAdapterConfig<CreateProductDto, Product>.NewConfig();
    TypeAdapterConfig<Product, UpdateProductDto>.NewConfig();
    TypeAdapterConfig<UpdateProductDto, Product>.NewConfig();
  }
}
