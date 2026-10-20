using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using VillaApi.Data;
using VillaApi.Models;

namespace VillaApi.Services
{
    public class VillaService : IVillaService
    {
        public readonly ApplicationDbContext _db;
        public readonly IMapper _map;
        public VillaService(ApplicationDbContext db, IMapper map)
        {
            _db = db;
            _map = map;
            
        }

        public async Task<IEnumerable<VillaDTo>> GetAllVillas()
        {
            var villas = await _db.Villas.ToListAsync();

            var responseVillaDto = _map.Map<List<VillaDTo>>(villas);

            return responseVillaDto;

        }

        public async Task<bool> GetVillaByName(string name)
        {
            var dublicateVilla = await _db.Villas.FirstOrDefaultAsync(v => v.Name == name);

            return dublicateVilla == null;
        }

        public async Task<Villa> GetVillaById(int id)
        {
            var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

            if (villa == null)
            {
                return null;
            }
            return villa;


        }

        public async Task<Villa> SaveVilla(VillaCreateDTO villaCreateDTO)
        {
            //Mapping
            Villa villa = _map.Map<Villa>(villaCreateDTO);


            await _db.Villas.AddAsync(villa);
            await _db.SaveChangesAsync();

            return villa;


        }

        public async Task DeleteVilla(Villa villa)
        {
             _db.Villas.Remove(villa);
            await _db.SaveChangesAsync();

            
        }

        public async Task UpdateVilla(Villa villa)
        {
            villa.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        public async Task<bool> GetVillaByNameAndId(string name, int id)
        {
            var dublicateVilla = await _db.Villas.FirstOrDefaultAsync(v => v.Name == name && v.Id != id);

            return dublicateVilla == null;


        }
    } 
}
