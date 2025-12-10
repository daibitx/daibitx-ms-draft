using Daibitx.EFCore.Extension.Abstractions;

namespace Daibitx.EFCore.Extension.Entity
{
    public abstract class SoftDeleteEntity : BaseEntity, ISoftDelete
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
