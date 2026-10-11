using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VillaApi.Data;
using VillaApi.Models;
using VillaApi.Models.DTOs;

namespace VillaApi.Services
{
    public class AuthService : IAuthService
    {

        ApplicationDbContext _db;
        IMapper _map;

        public AuthService(IMapper map , IConfigurationManager configurationManager , ApplicationDbContext db)
        {
                _map = map;
            _db = db;

        }
        public async Task<bool> IsEmailExsistsAsync(string email)
        {
            return await _db.Users.AnyAsync(u=>u.Email.Equals(email,StringComparison.CurrentCultureIgnoreCase));
        }

        public Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            throw new NotImplementedException();
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
    }
}
