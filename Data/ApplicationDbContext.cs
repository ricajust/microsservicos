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

            // Configuração para garantir que o CPF seja único na tabela Usuarios
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Cpf)
                .IsUnique();
                }
    }
}