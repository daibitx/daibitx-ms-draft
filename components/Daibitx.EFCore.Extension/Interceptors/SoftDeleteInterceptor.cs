using Daibitx.EFCore.Extension.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Daibitx.EFCore.Extension.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ConvertDeletesToSoftDeletes(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        private void ConvertDeletesToSoftDeletes(DbContext? context)
        {
            if (context == null) return;

            var entries = context.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Deleted && x.Entity is ISoftDelete);

            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;

                var entity = (ISoftDelete)entry.Entity;
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
