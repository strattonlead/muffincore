using System;

namespace Muffin.Vimeo.Abstraction
{
    public interface IVimeoToken
    {
        long? ExternalId { get; set; }
        string Name { get; set; }
        string ExternalProfileImageUrl { get; set; }
        DateTime AccountCreatedDateutc { get; set; }
        string AccessToken { get; set; }
        string Scope { get; set; }
        string TokenType { get; set; }
    }
}
