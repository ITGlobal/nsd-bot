using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NSD.Bot2.Models
{
    public class KnowledgeBasesContext : DbContext
    {
        public DbSet<KB> KB { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=KnowledgeBases;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KB>(entity =>
            {
                entity.ToTable("KB");

                entity.Property(e => e.KBId)
                    .HasColumnName("KBId")
                    .ValueGeneratedNever();

                entity.Property(e => e.KBName)
                    .IsRequired()
                    .HasColumnName("KBName");
            });
        }
    }
}
