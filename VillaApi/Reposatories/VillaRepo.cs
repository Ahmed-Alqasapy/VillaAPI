using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using System.Linq.Expressions;
using VillaApi.Data;
using VillaApi.Models;

namespace VillaApi.Reposatories
{
    public class VillaRepo(ApplicationDbContext _db)
    {
        public async Task<ApiPagedResult<Villa>> FindVillas(VillaQueryParams q)
        {

            var page = q.Page;

            var pageSize = Math.Clamp(q.PageSize, 1, 100);

            IQueryable<Villa> query = _db.Villas.AsNoTracking();

            #region FILER LATER BETTER APPROACH

            //APlly Filtering 
            if (q.MinRate is not null) query = query.Where(v => v.Rate >= q.MinRate);
            if (q.MaxRate is not null) query = query.Where(v => v.Rate <= q.MaxRate);
            if (q.Occupancy is not null) query =query.Where(v => v.Occupancy >= q.Occupancy);
            //Search by name
            if (q.Search is not null) query = query.Where(v => v.Name.Contains(q.Search));

 
            #endregion

            //Sorting  and paginatiing
            //Fix Count To Count Async
            var total = await query!.CountAsync();

            //Sorting  With new Switch Approach
            Expression<Func<Villa, object>> KeyValueSort = q.SortBy switch
            {
                "rate" => V => V.Rate,
                "name" => V => V.Name,
                "occupancy" => V => V.Occupancy,
                "created" => V => V.CreatedDate,
                _ => V=>V.Id
            };
            //it is like e = v=>v.rate;

            bool desc = String.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

            query = desc
                ? query.OrderByDescending(keySelector: KeyValueSort).ThenByDescending(v => v.Id) 
                :query.OrderBy(keySelector: KeyValueSort).ThenBy(v => v.Id);

    
            var items = await query
                .Skip((page-1)*pageSize)
                .Take(pageSize)
                .ToListAsync();
         
            

               
            return new ApiPagedResult<Villa> { Page= page , PageSize = pageSize , TotalItems = total , Items = items };

            
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
