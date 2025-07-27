using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository;

public class UserRepository : IUserRepository
{

  public readonly ApplicationDbContext _db;
  private string? secretKey;

  public UserRepository(ApplicationDbContext db, IConfiguration configuration)
  {
    _db = db;
    secretKey = configuration.GetValue<string>("ApiSettings:SecretKey"); // Para leer la variable de entorno.
  }

  public User? GetUser(int id)
  {
    return _db.Users.FirstOrDefault(u => u.Id == id);
  }

  public ICollection<User> GetUsers()
  {
    return _db.Users.OrderBy(u => u.Username).ToList();
  }

  public bool IsUniqueUser(string username)
  {
    return !_db.Users.Any(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
  }

  public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
  {
    if (string.IsNullOrEmpty(userLoginDto.Username)) return UserResponse("", "El username es obligatorio.");

    var user = await _db.Users.FirstOrDefaultAsync<User>(u => u.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

    if (user == null) return UserResponse("", "El username no encontrado.");
    if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password)) return UserResponse("", "La contraseña es incorrecta.");

    // JWT Generate
    var handlerToken = new JwtSecurityTokenHandler();
    if (string.IsNullOrWhiteSpace(secretKey)) throw new InvalidOperationException("El SecretKey no está configurada.");

    var key = Encoding.UTF8.GetBytes(secretKey);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new[]
      {
        new Claim("id", user.Id.ToString()),
        new Claim("username", user.Username),
        new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
      }),
      Expires = DateTime.UtcNow.AddHours(2),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = handlerToken.CreateToken(tokenDescriptor);

    return UserResponse(
      handlerToken.WriteToken(token),
      "Usuario Logeado Correctamente.",
      new UserRegisterDto() { Username = user.Username, Name = user.Name, Role = user.Role, Password = user.Password ?? "" }
    );
  }

  public async Task<User> Register(CreateUserDto createUserDto)
  {
    var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
    var user = new User()
    {
      Username = createUserDto.Username ?? "No Username",
      Name = createUserDto.Name,
      Password = encriptedPassword,
      Role = createUserDto.Role,
    };

    _db.Users.Add(user);

    await _db.SaveChangesAsync();

    return user;
  }

  // Método para regresar un mensaje personalizado.
  private UserLoginResponseDto UserResponse(string token, string message, UserRegisterDto? user = null)
  {
    return new UserLoginResponseDto()
    {
      Token = token,
      User = user,
      Message = message
    };
  }
}
