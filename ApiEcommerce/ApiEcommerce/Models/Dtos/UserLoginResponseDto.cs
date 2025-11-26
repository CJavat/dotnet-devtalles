namespace ApiEcommerce.Models.Dtos;

public class UserLoginResponseDto
{
  public UserLoginDto? User { get; set; }
  public string? Token { get; set; }
  public string? Message { get; set; }
}
