using Estoque.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estoque.Infrastructure.Data.Configurations;

public sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("CP5_FORNECEDORES");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedOnAdd();
        builder.Property(f => f.RazaoSocial).HasMaxLength(150).IsRequired();
        builder.Property(f => f.Cnpj).HasMaxLength(14).IsRequired();
        builder.Property(f => f.Email).HasMaxLength(150).IsRequired();
        builder.Property(f => f.Telefone).HasMaxLength(20);
        builder.Property(f => f.Cep).HasMaxLength(8).IsRequired();
        builder.Property(f => f.Logradouro).HasMaxLength(150);
        builder.Property(f => f.Bairro).HasMaxLength(100);
        builder.Property(f => f.Cidade).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Uf).HasMaxLength(2).IsRequired();
        builder.Property(f => f.CriadoEm).IsRequired();

        builder.HasIndex(f => f.Cnpj).IsUnique();
        builder.HasIndex(f => f.Uf);

        builder.HasMany(f => f.Produtos)
            .WithOne(p => p.Fornecedor)
            .HasForeignKey(p => p.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("CP5_PRODUTOS");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Nome).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(500);
        builder.Property(p => p.Sku).HasMaxLength(40).IsRequired();
        builder.Property(p => p.Preco).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.QuantidadeEstoque).IsRequired();
        builder.Property(p => p.CriadoEm).IsRequired();

        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Nome);
        builder.HasIndex(p => p.FornecedorId);
    }
}

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("CP5_USUARIOS");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();
        builder.Property(u => u.Username).HasMaxLength(50).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Perfil).HasMaxLength(20).IsRequired();
        builder.Property(u => u.CriadoEm).IsRequired();

        builder.HasIndex(u => u.Username).IsUnique();
    }
}
