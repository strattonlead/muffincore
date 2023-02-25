using System.Linq;

namespace CreateIf.Instagram.Services
{
    /// <summary>
    /// Enumarator Service für die Access Codes
    /// </summary>
    public interface IInstagramAccessTokenService
    {
        IQueryable<IAccessToken> AccessTokens { get; }

        void UpdateAccessToken(IAccessToken accessToken);
    }
}