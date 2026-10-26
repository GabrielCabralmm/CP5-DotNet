using Estoque.Application.Interfaces;
using Estoque.Domain.Entities;
using Estoque.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Estoque.Infrastructure.Data;

public sealed class DatabaseInitializer(EstoqueDbContext context, IPasswordHasher hasher, ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var creator = context.GetService<IRelationalDatabaseCreator>();

        if (!await creator.ExistsAsync(ct))
        {
            logger.LogInformation("Banco de dados não encontrado. Criando banco e tabelas.");
            await creator.CreateAsync(ct);
            await creator.CreateTablesAsync(ct);
        }
        else if (!await TabelasExistemAsync(ct))
        {
            logger.LogInformation("Tabelas do CP5 não encontradas. Criando tabelas.");
            await creator.CreateTablesAsync(ct);
        }

        await SeedUsuariosAsync(ct);
        await SeedCatalogoAsync(ct);
    }

    private async Task<bool> TabelasExistemAsync(CancellationToken ct)
    {
        try
        {
            await context.Usuarios.AsNoTracking().AnyAsync(ct);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task SeedUsuariosAsync(CancellationToken ct)
    {
        if (await context.Usuarios.AnyAsync(ct))
            return;

        context.Usuarios.AddRange(
            new Usuario("admin", hasher.Hash("Admin@123"), Perfis.Admin),
            new Usuario("leitor", hasher.Hash("Leitor@123"), Perfis.Leitor));

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Usuários padrão criados (admin / leitor).");
    }

    private async Task SeedCatalogoAsync(CancellationToken ct)
    {
        if (await context.Fornecedores.AnyAsync(ct))
            return;

        var fornecedores = new[]
        {
            new Fornecedor("Distribuidora Paulista de Eletrônicos LTDA", "11222333000181", "contato@paulistaeletronicos.com.br", "(11) 3333-1000",
                new Endereco("01310100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP")),
            new Fornecedor("Papelaria Carioca Atacado S.A.", "22333444000172", "vendas@papelariacarioca.com.br", "(21) 2222-2000",
                new Endereco("20040002", "Rua da Assembleia", "Centro", "Rio de Janeiro", "RJ")),
            new Fornecedor("Mineira Utilidades Domésticas LTDA", "33444555000163", "comercial@mineirautilidades.com.br", "(31) 3111-3000",
                new Endereco("30130010", "Praça Sete de Setembro", "Centro", "Belo Horizonte", "MG"))
        };

        context.Fornecedores.AddRange(fornecedores);
        await context.SaveChangesAsync(ct);

        var produtos = new[]
        {
            new Produto("Mouse sem fio", "Mouse óptico 1600 DPI", "ELE-MOUSE-001", 89.90m, 120, fornecedores[0].Id),
            new Produto("Teclado mecânico", "Switch azul ABNT2", "ELE-TECL-002", 349.90m, 45, fornecedores[0].Id),
            new Produto("Monitor 24 polegadas", "Full HD IPS 75Hz", "ELE-MONI-003", 899.00m, 20, fornecedores[0].Id),
            new Produto("Headset USB", "Com microfone e cancelamento de ruído", "ELE-HEAD-004", 259.90m, 60, fornecedores[0].Id),
            new Produto("Webcam HD", "1080p com microfone", "ELE-WEBC-005", 199.90m, 35, fornecedores[0].Id),
            new Produto("Caderno universitário", "10 matérias 200 folhas", "PAP-CADE-001", 32.50m, 500, fornecedores[1].Id),
            new Produto("Caneta esferográfica azul", "Caixa com 50 unidades", "PAP-CANE-002", 45.00m, 300, fornecedores[1].Id),
            new Produto("Papel sulfite A4", "Resma com 500 folhas", "PAP-SULF-003", 29.90m, 800, fornecedores[1].Id),
            new Produto("Marca-texto", "Kit com 6 cores", "PAP-MARC-004", 24.90m, 250, fornecedores[1].Id),
            new Produto("Garrafa térmica 1L", "Aço inox", "UTI-GARR-001", 79.90m, 90, fornecedores[2].Id),
            new Produto("Jogo de panelas", "5 peças antiaderente", "UTI-PANE-002", 299.00m, 25, fornecedores[2].Id),
            new Produto("Organizador multiuso", "Plástico transparente 10L", "UTI-ORGA-003", 39.90m, 150, fornecedores[2].Id)
        };

        context.Produtos.AddRange(produtos);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Catálogo inicial criado com {Fornecedores} fornecedores e {Produtos} produtos.", fornecedores.Length, produtos.Length);
    }
}
