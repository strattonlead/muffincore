using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CreateIf.Instagram.Services
{
    public class InstagramOAuth2RefreshService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly InstagramOAuth2ServiceOptions _options;
        private readonly ILogger _logger;

        public InstagramOAuth2RefreshService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _options = serviceProvider.GetRequiredService<InstagramOAuth2ServiceOptions>();
            _logger = serviceProvider.GetRequiredService<ILogger<InstagramOAuth2RefreshService>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var oAuth2Service = scope.ServiceProvider.GetRequiredService<IInstagramOAuth2Service>();
                        var accessTokenService = scope.ServiceProvider.GetRequiredService<IInstagramAccessTokenService>();
                        foreach (var accessToken in accessTokenService.AccessTokens)
                        {
                            if (accessToken.IsValid && accessToken.Expires <= _options.MinAccessTokenLifeSpan)
                            {
                                var newToken = await oAuth2Service.RefreshAccessToken(accessToken);
                                accessTokenService.UpdateAccessToken(newToken);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while refreshing access token");
                }

                try
                {
                    await Task.Delay(_options.RefreshServiceInterval, stoppingToken);
                }
                catch { }
            }
        }
    }
}
