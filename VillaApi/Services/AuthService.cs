using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VillaApi.Data;
using VillaApi.Models;
using VillaApi.Models.DTOs;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace VillaApi.Services
{
    public class AuthService : IAuthService
    {

        ApplicationDbContext _db;
        IMapper _map;
        IConfiguration _config;

        public AuthService(IMapper map , IConfiguration configuration , ApplicationDbContext db)
        {
                _map = map;
            _config = configuration;
                _db = db;

        }
        public async Task<bool> IsEmailExsistsAsync(string email)
        {
            return await _db.Users.AnyAsync(u=>u.Email.ToLower()==email.ToLower());
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            //throw new NotImplementedException();
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());

                if (user == null || user.Password != loginRequestDTO.Password)
                {


                    return null;
                }

                //Generate JWT Token here and return it along with user details

                return new LoginResponseDTO
                {
                    Token = "",
                    UserDto = _map.Map<UserDTO>(user)
                };
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("there is an error while registering the user", ex);
            }


        }

        public async Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                var exist = await IsEmailExsistsAsync(registrationRequestDTO.Email);
                if (exist)
                {
                    throw new InvalidOperationException("Email already exists");
                }

                User user = new User()
                {
                    Email = registrationRequestDTO.Email,
                    Name = registrationRequestDTO.Name,
                    Password = registrationRequestDTO.Password,
                    Role = string.IsNullOrEmpty(registrationRequestDTO.Role) ? "Customer" : registrationRequestDTO.Role,
                    CreatedDate = DateTime.Now
                };


                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                return _map.Map<UserDTO>(user);

            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("there is an error while registering the user", ex);
            }
          


        }



         public string GenerateJwtToken(User user)
        {
            var key =Encoding.ASCII.GetBytes( _config.GetSection("JwtSettings")["Secret_Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                Expires= DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
                
            };

            var tokenHandler = new JsonWebTokenHandler();

            //var token = tokenHandler.CreateToken(tokenDescriptor);

            //var jwt = tokenHandler.vre(token);
            var jwt = tokenHandler.CreateToken(tokenDescriptor);

            return jwt;
        }

    }
}
