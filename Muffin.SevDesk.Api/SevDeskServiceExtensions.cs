using Microsoft.Extensions.DependencyInjection;
using System;

namespace Muffin.SevDesk.Api
{
    public static class SevDeskServiceExtensions
    {
        public static void AddSevDeskService(this IServiceCollection services, Action<SevDeskServiceOptionsBuilder> builder)
        {
            var optionsBuilder = new SevDeskServiceOptionsBuilder();
            builder?.Invoke(optionsBuilder);

            var options = optionsBuilder.Build();
            services.AddSingleton(options);
            services.AddScoped<ISevDeskService, SevDeskService>();
        }
    }
}
