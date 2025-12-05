using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersionNeutral]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class UsersController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        // Eliminado IMapper, Mapster se usa de forma estática

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            var usersDto = users.Adapt<List<UserDto>>();

            return Ok(usersDto);
        }

        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUser(string id)
        {
            var user = _userRepository.GetUser(id);
            if (user == null) return NotFound($"El usuario con el id {id} no existe.");
            var userDto = user.Adapt<UserDto>();

            return Ok(userDto);
        }

        [AllowAnonymous]
        [HttpPost("Register", Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null || !ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrEmpty(createUserDto.Username)) return BadRequest("Username es requerido.");

            if (!_userRepository.IsUniqueUser(createUserDto.Username)) return BadRequest("El usuario ya existe.");

            var result = await _userRepository.Register(createUserDto);
            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al registrar el usuario.");

            return CreatedAtRoute("GetUser", new { id = result.Id }, result);
        }

        [AllowAnonymous]
        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        {
            if (userLoginDto == null || !ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userRepository.Login(userLoginDto);
            if (user == null)
                return Unauthorized();

            return Ok(user);
        }


    }
}
