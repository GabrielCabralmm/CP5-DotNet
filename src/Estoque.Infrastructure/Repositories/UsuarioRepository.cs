using Estoque.Domain.Entities;
using Estoque.Domain.Interfaces;
using Estoque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure.Repositories;

public sealed class UsuarioRepository(EstoqueDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, ct);
}
