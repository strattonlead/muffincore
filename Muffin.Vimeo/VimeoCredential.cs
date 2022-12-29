using Muffin.Vimeo.Abstraction;

namespace Muffin.Vimeo
{
    public class VimeoCredential : IVimeoCredential
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
