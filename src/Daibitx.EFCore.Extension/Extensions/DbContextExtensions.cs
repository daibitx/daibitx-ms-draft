using Microsoft.EntityFrameworkCore;

namespace Daibitx.EFCore.Extension.Extensions
{
    public static class DbContextExtensions
    {
        public static Task ExecuteTransactionAsync(
            this DbContext context,
            Func<Task> action)
        {
            return context.Database.CreateExecutionStrategy()
                .ExecuteAsync(async () =>
                {
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    await action();
                    await transaction.CommitAsync();
                });
        }

        public static Task BulkInsertAsync<T>(this DbContext context, IEnumerable<T> items)
            where T : class
        {
            context.Set<T>().AddRange(items);
            return context.SaveChangesAsync();
        }
    }
}
