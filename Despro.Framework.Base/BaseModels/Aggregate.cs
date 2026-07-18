using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Despro.Framework.Base.BaseModels;

public abstract class Aggregate
{
    [Key, Column(Order = 0)]
    public long Id { get; private set; }
    [Column(Order = 1)]
    public bool IsDelete { get; private set; }
    public long? CreateDate { get; private set; }
    public long? CreateUserId { get; private set; }
    public long? UpdateDate { get; private set; }
    public long? UpdateUserId { get; private set; }
    public long? DeleteDate { get; private set; }
    public long? DeleteUserId { get; private set; }


    public void SetId(long id)
    {
        Id = id;
    }

    public void SetCreate(long createDate, long createUserId)
    {
        CreateDate = createDate;
        CreateUserId = createUserId;
    }

    public void SetUpdate(long updateDate, long updateUserId)
    {
        UpdateDate = updateDate;
        UpdateUserId = updateUserId;
    }

    public void SetDelete(long deleteDate, long deleteUserId)
    {
        DeleteDate = deleteDate;
        DeleteUserId = deleteUserId;
        IsDelete = true;
    }
}