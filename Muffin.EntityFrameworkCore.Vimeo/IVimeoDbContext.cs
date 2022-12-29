using Microsoft.EntityFrameworkCore;
using Muffin.Vimeo.Abstraction;

namespace Muffin.EntityFrameworkCore.Vimeo
{
    public interface IVimeoDbContext<TVimeoCredential, TVimeoToken>
        where TVimeoCredential : class, IVimeoCredential
        where TVimeoToken : class, IVimeoToken
    {
        DbSet<TVimeoCredential> VimeoCredentials { get; set; }
        DbSet<TVimeoToken> VimeoTokens { get; set; }
    }

    public interface IVimeoDbContext : IVimeoDbContext<VimeoCredential, VimeoToken>
    {
    }
}
