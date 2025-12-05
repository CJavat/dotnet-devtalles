
using ApiEcommerce.Models.Dtos;
using Mapster;

namespace ApiEcommerce.Mapping;

public static class CategoryMappingConfig
{
  public static void Register()
  {
    TypeAdapterConfig<Category, CategoryDto>.NewConfig();
    TypeAdapterConfig<CategoryDto, Category>.NewConfig();
    TypeAdapterConfig<Category, CreateCategoryDto>.NewConfig();
    TypeAdapterConfig<CreateCategoryDto, Category>.NewConfig();
  }
}
