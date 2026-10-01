using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Despro.Framework.Base.BaseModels;

public abstract class Aggregate<TId> where TId : notnull
{
    protected Aggregate() { }

    [Key, Column(Order = 0)]
    public TId Id { get; protected set; } = default!;
    [Column(Order = 1)]
    public bool IsDelete { get; private set; }

    public void SetId(TId id)
    {
        Id = id;
    }

    public void SetDelete()
    {
        IsDelete = true;
    }
}