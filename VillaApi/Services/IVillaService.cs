using RoyalVilla.DTO;
using VillaApi.Models;

namespace VillaApi.Services
{
    public interface IVillaService
    {
        public  Task<ApiPagedResult<VillaDTo>> GetAllVillas(VillaQueryParams villaQueryParams);

        public Task<Villa> GetVillaById(int id);

        public Task<bool> GetVillaByName(string name);
        public Task<bool> GetVillaByNameAndId(string name, int id);

        public Task<Villa> SaveVilla(VillaCreateDTO villaCreateDTO);

        public Task DeleteVilla(Villa villa);
        public Task UpdateVilla(Villa villa);
    }
}
