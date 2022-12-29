using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Muffin.SSH
{
    public static class SftpClientExtensions
    {
        #region Upload

        public static async Task UploadFileAsync(this SftpClient client, Stream input, string path)
        {
            await Task.Factory.FromAsync(client.BeginUploadFile(input, path), x => client.EndUploadFile(x));
        }

        public static async Task UploadFileAsync(this SftpClient client, Stream input, string path, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null)
        {
            await Task.Factory.FromAsync(client.BeginUploadFile(input, path, asyncCallback, state, uploadCallback), x => client.EndUploadFile(x));
        }

        public static async Task UploadFileAsync(this SftpClient client, Stream input, string path, bool canOverride, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null)
        {
            await Task.Factory.FromAsync(client.BeginUploadFile(input, path, canOverride, asyncCallback, state, uploadCallback), x => client.EndUploadFile(x));
        }

        #endregion

        #region Download

        public static async void DownloadAsync(this SftpClient client, string path, Stream output)
        {
            await Task.Factory.FromAsync(client.BeginDownloadFile(path, output), x => client.EndDownloadFile(x));
        }

        public static async void DownloadAsync(this SftpClient client, string path, Stream output, AsyncCallback asyncCallback, object state, Action<ulong> downloadCallback = null)
        {
            await Task.Factory.FromAsync(client.BeginDownloadFile(path, output, asyncCallback, state, downloadCallback), x => client.EndDownloadFile(x));
        }

        public static async void DownloadAsync(this SftpClient client, string path, Stream output, AsyncCallback asyncCallback)
        {
            await Task.Factory.FromAsync(client.BeginDownloadFile(path, output, asyncCallback), x => client.EndDownloadFile(x));
        }

        #endregion

        #region Directory Listing

        public static async Task<IEnumerable<SftpFile>> ListDirectoriesAsync(this SftpClient client, string path, AsyncCallback asyncCallback, object state, Action<int> listCallback = null)
        {
            return await Task.Factory.FromAsync(client.BeginListDirectory(path, asyncCallback, state, listCallback), x => client.EndListDirectory(x));
        }

        #endregion

        #region Synchronize Listing

        public static async Task<IEnumerable<FileInfo>> SynchronizeDirectoriesAsync(this SftpClient client, string sourcePath, string destinationPath, string searchPattern, AsyncCallback asyncCallback, object state)
        {
            return await Task.Factory.FromAsync(client.BeginSynchronizeDirectories(sourcePath, destinationPath, searchPattern, asyncCallback, state), x => client.EndSynchronizeDirectories(x));
        }

        #endregion
    }
}
