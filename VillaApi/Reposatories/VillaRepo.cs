using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using VillaApi.Data;
using VillaApi.Models;

namespace VillaApi.Reposatories
{
    public class VillaRepo(ApplicationDbContext _db)
    {
        public async Task<ApiPagedResult<Villa>> FindVillas(VillaQueryParams q)
        {

            var page = q.Page;

            var pageSize = q.PageSize;

            //var villas = await _db.Villas.OrderBy(x => x.Id).ToListAsync();

            IQueryable<Villa> query = _db.Villas.AsNoTracking();


            #region FILER LATER BETTER APPROACH
            ////Filter
            //if (query is not null )
            //{
            //     query = _db.Villas.Where(v=>v.Occupancy == q.Ocuuoancy)
            //} 
            #endregion

            //Sorting  and paginatiing

            var total = query.Count();

            var items = await query.OrderByDescending(v => v.Rate).ThenByDescending(v => v.Id)
                .Skip((page-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();


               
            return new ApiPagedResult<Villa> { Page= page , PageSize = pageSize , TotalItems = total , Items = items };

            //return items;






            //return  await _db.Villas.ToListAsync();
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
