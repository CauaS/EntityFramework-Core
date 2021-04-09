using CursoEFCore.Data.Configurations;
using CursoEFCore.DomainEF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursoEFCore.Data
{
    class ApplicationContext : DbContext
    {
        public DbSet<Pedido> Pedido { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /* Por meio do optionsBuilder que vamos dizer qual o provider iremos usar...*/
            optionsBuilder.UseSqlServer("Data source=(localdb)\\mssqllocaldb;Initial Catalog=CursoEFCore;Integrated Security=true",
                p => p.EnableRetryOnFailure( // por padrão tenta duas vezes
                    maxRetryCount : 2, // seta o nro maximo de tentativas
                    maxRetryDelay: TimeSpan.FromSeconds(5), // intervalo entre as tentativas
                    errorNumbersToAdd: null //quais erro a leval em consideração
                ).MigrationsHistoryTable("NomeDaMigration"));

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationContext).Assembly);
        }
    }
}
