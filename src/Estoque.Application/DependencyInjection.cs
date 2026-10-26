using Estoque.Application.Interfaces;
using Estoque.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Estoque.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
