using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Mapster;

namespace ApiEcommerce.Mapping;

public static class UserMappingConfig
{
  public static void Register()
  {
    TypeAdapterConfig<User, UserDto>.NewConfig();
    TypeAdapterConfig<UserDto, User>.NewConfig();
    TypeAdapterConfig<User, CreateUserDto>.NewConfig();
    TypeAdapterConfig<CreateUserDto, User>.NewConfig();
    TypeAdapterConfig<User, UserLoginDto>.NewConfig();
    TypeAdapterConfig<UserLoginDto, User>.NewConfig();
    TypeAdapterConfig<User, UserLoginResponseDto>.NewConfig();
    TypeAdapterConfig<UserLoginResponseDto, User>.NewConfig();
    TypeAdapterConfig<ApplicationUser, UserDataDto>.NewConfig();
    TypeAdapterConfig<UserDataDto, ApplicationUser>.NewConfig();
    TypeAdapterConfig<ApplicationUser, UserDto>.NewConfig();
    TypeAdapterConfig<UserDto, ApplicationUser>.NewConfig();
  }
}
