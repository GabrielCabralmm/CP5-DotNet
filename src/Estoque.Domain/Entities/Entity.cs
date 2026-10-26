namespace Estoque.Domain.Entities;

public abstract class Entity
{
    public int Id { get; protected set; }

    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;

    public DateTime? AtualizadoEm { get; protected set; }

    protected void MarcarAtualizacao() => AtualizadoEm = DateTime.UtcNow;
}
