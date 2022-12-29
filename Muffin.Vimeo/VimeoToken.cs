using Muffin.Vimeo.Abstraction;
using System;

namespace Muffin.Vimeo
{
    public class VimeoToken : IVimeoToken
    {
        public long? ExternalId { get; set; }
        public string Name { get; set; }
        public string ExternalProfileImageUrl { get; set; }
        public DateTime AccountCreatedDateutc { get; set; }
        public string AccessToken { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
    }
}
