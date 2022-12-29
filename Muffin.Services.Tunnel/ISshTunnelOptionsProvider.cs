namespace Muffin.Services.Tunnel
{
    public interface ISshTunnelOptionsProvider
    {
        Task<GetSshTunnelOptions> GetOptions(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }

    public class GetSshTunnelOptions
    {
        public SshTunnelOptions Options { get; set; }
        public bool Succss => Options != null;
        public bool StopService { get; set; }
    }

    public class SshTunnelOptions
    {
        public string Username { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string PrivateKeyFilePath { get; set; }
        public string PrivateKeyString { get; set; }
        public uint? LocalPort { get; set; }
        public uint RemotePort { get; set; }
    }
}
