using System;
using Microsoft.Extensions.DependencyInjection;

namespace CreateIf.Instagram.Services
{
    /// <summary>
    /// DI extensions
    /// </summary>
    public static class InstagramOAuth2ServiceExtensions
    {
        public static void AddInstagramOAuth2Service(this IServiceCollection services)
        {
            services.AddInstagramOAuth2Service(x => { });
        }

        public static void AddInstagramOAuth2Service(this IServiceCollection services, Action<InstagramOAuth2ServiceOptionsBuilder> builder)
        {
            var optionsBuilder = new InstagramOAuth2ServiceOptionsBuilder();
            builder?.Invoke(optionsBuilder);
            var options = optionsBuilder.Build();

            services.AddSingleton(options);
            services.AddSingleton(options.Events);
            services.AddScoped<IInstagramOAuth2Service, InstagramOAuth2Service>();

            if (options.UseRefreshService)
            {
                services.AddScoped(options.RefreshServiceType, options.RefreshServiceImplementationType);
                services.AddHostedService<InstagramOAuth2RefreshService>();
            }
        }
    }
}