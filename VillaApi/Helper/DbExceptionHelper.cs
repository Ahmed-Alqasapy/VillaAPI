using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace VillaApi.Helper
{
    public static class DbExceptionHelper
    {
        public static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is SqlException sql
           && (sql.Number == 2601 || sql.Number == 2627);
    }
}
