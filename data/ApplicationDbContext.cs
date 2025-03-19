using Microsoft.EntityFrameworkCore;
using Alunos.API.Models;

namespace Alunos.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Aluno> Alunos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aqui você pode adicionar configurações específicas para o seu modelo
            // Por exemplo, configurar relacionamentos, chaves compostas, etc.
        }
    }
}