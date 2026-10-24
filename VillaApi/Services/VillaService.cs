using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using VillaApi.Data;
using VillaApi.Models;
using VillaApi.Reposatories;

namespace VillaApi.Services
{
    public class VillaService : IVillaService
    {
        public readonly ApplicationDbContext _db;
        public readonly IMapper _map;
        public readonly VillaRepo _villaRepo;
        public VillaService(ApplicationDbContext db, IMapper map , VillaRepo villaRepo)
        {
            _db = db;
            _map = map;
            _villaRepo = villaRepo;
            
        }

        public async Task<IEnumerable<VillaDTo>> GetAllVillas()
        {
            var villas = await _villaRepo.FindVillas();

            var responseVillaDto = _map.Map<List<VillaDTo>>(villas);

            return responseVillaDto;

        }

        public async Task<bool> GetVillaByName(string name)
        {
            var dublicateVilla =await _villaRepo.FindVillaByName(name);

            return dublicateVilla == null ? false : true;
        }

        public async Task<Villa> GetVillaById(int id)
        {
            var villa = await _villaRepo.FindVillaById(id);

            if (villa == null)
            {
                return null;
            }
            return villa;


        }

        public async Task<Villa> SaveVilla(VillaCreateDTO villaCreateDTO)
        {

            Villa villa = _map.Map<Villa>(villaCreateDTO);

            await _villaRepo.SavingVilla(villa);

            return villa;


        }

        public async Task DeleteVilla(Villa villa)
        {
           

            await _villaRepo.DeletingVilla(villa);
            
        }

        public async Task UpdateVilla(Villa villa)
        {
            villa.UpdatedDate = DateTime.Now;
            await _villaRepo.UpdatingVilla();
        }

        public async Task<bool> GetVillaByNameAndId(string name, int id)
        {
            var dublicateVilla = await _villaRepo.GetVillaByNameAndId(name,id);

            return dublicateVilla == null;


        }
    } 
}
