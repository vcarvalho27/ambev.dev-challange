using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.WebApi.Extentions;

public static class ServiceProviderExtention
{
    public static IServiceProvider ApplyMigration(this IServiceProvider serviceCollection)
    {
        using (var scope = serviceCollection.CreateScope())
        {
            var _Db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            if (_Db != null)
            {
                if (_Db.Database.GetPendingMigrations().Any())
                {
                    _Db.Database.Migrate();
                }
            }
        }

        return serviceCollection;
    }
}
