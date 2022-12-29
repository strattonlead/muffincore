namespace Muffin.Vimeo.Abstraction
{
    public interface IVimeoCredential
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
    }
}
