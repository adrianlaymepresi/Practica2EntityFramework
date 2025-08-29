using Microsoft.EntityFrameworkCore;
using EntityFrameworkSQLserver.Models;

namespace EntityFrameworkSQLserver.Data
{
    public class TareaDbContext : DbContext
    {
        public TareaDbContext(DbContextOptions<TareaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tarea> Tareas { get; set; }
    }
}
