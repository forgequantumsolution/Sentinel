using Microsoft.Extensions.DependencyInjection;

namespace Analytics_BE.Infrastructure.Extensions
{
    public static class Provider
    {
        private static IServiceScopeFactory? _scopeFactory;

        public static void SetProvider(IServiceProvider provider) => _scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        public static T Get<T>() where T : notnull
        {
            if (_scopeFactory == null)
                throw new InvalidOperationException("Service provider has not been initialized");

            var scope = _scopeFactory.CreateScope();
            try
            {
                return scope.ServiceProvider.GetRequiredService<T>();
            }
            catch
            {
                scope.Dispose();
                throw;
            }
        }

        public static async Task ExecuteInScopeAsync(Func<IServiceProvider, Task> action)
        {
            if (_scopeFactory == null)
                throw new InvalidOperationException("Service provider has not been initialized");

            using var scope = _scopeFactory.CreateScope();
            await action(scope.ServiceProvider);
        }
    }
}
