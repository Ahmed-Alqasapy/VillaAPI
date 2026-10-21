using Microsoft.EntityFrameworkCore;
using VillaApi.Data;
using VillaApi.Models;

namespace VillaApi.Reposatories
{
    public class VillaRepo(ApplicationDbContext _db)
    {
        public async Task<List<Villa>> FindVillas()
        {


            return  await _db.Villas.ToListAsync();
        }

        public async Task<Villa> FindVillaById(int id)
        {
            return await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Villa> FindVillaByName(string name)
        {
           return await _db.Villas.FirstOrDefaultAsync(v => v.Name == name);
        }

        public async Task<Villa> GetVillaByNameAndId(string name, int id)
        {
            return await _db.Villas.FirstOrDefaultAsync(v => v.Name == name && v.Id != id);
        }

        public async Task SavingVilla(Villa villa)
        {

            await _db.Villas.AddAsync(villa);
            await _db.SaveChangesAsync();
        }

        public async Task DeletingVilla(Villa villa)
        {
            _db.Villas.Remove(villa);
          await  _db.SaveChangesAsync();
        }

        public async Task UpdatingVilla()
        {
            await _db.SaveChangesAsync();
        }
    }
}
