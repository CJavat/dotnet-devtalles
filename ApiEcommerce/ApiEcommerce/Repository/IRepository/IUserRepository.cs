using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;

namespace ApiEcommerce.Repository.IRepository;
/*
=============
🏆 Ejercicio 
=============
*/
// 2. Incluir los siguientes métodos en la interfaz:
//    - Login
//        → Recibe un objeto UserLoginDto y devuelve un UserLoginResponseDto de forma asíncrona (Task).
//
//    - Register
//        → Recibe un objeto CreateUserDto y devuelve un objeto User de forma asíncrona (Task).

public interface IUserRepository
{
  ICollection<User> GetUsers();
  User? GetUser(int id);
  bool IsUniqueUser(string username);
  Task<UserLoginResponseDto> Login(UserLoginDto userLogin);
  Task<User> Register(CreateUserDto createUserDto);
}
