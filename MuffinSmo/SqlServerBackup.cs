using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace MuffinSmo
{


    public class SqlServerBackup
    {
        /// <summary>
        /// Wird beim Fortschritt des Backup oder Restore aufgerufen
        /// </summary>
        public event PercentCompleteEventHandler
           PercentComplete;

        /// <summary>
        /// Erzeugt ein Backup einer Datenbank 
        /// </summary>
        /// <param name="serverName">Der Name des SQL Servers</param>
        /// <param name="databaseName">Der Name der zu sichernden Datenbank</param>
        /// <param name="backupFileName">Der Name der Datei, in die das Backup geschrieben werden soll</param>
        /// <param name="useTrustedConnection">Gibt an, ob eine vertraute Verbindung verwendet werden soll</param>
        /// <param name="login">Login-Name für den Login falls keine vertraute Verbindung verwendet werden soll</param>
        /// <param name="password">Passwort für den Login falls keine vertraute Verbindung verwendet werden soll</param>
        public void CreateBackupToFile(string serverName, string databaseName,
           string backupFileName, bool useTrustedConnection, string login,
           string password)
        {
            // Verbindungsinformationen definieren
            ServerConnection serverConnection = new ServerConnection();
            serverConnection.ServerInstance = serverName;
            if (useTrustedConnection)
            {
                serverConnection.LoginSecure = true;
            }
            else
            {
                serverConnection.LoginSecure = false;
                serverConnection.Login = login;
                serverConnection.Password = password;
            }

            // Verbindung aufbauen
            Server server = new Server(serverConnection);
            try
            {
                server.ConnectionContext.Connect();

                // Backup in die angegebene Datei erstellen
                Backup backup = new Backup();
                backup.Action = BackupActionType.Files;
                backup.Database = databaseName;
                if (this.PercentComplete != null)
                {
                    backup.PercentComplete += this.PercentComplete;
                }
                backup.Devices.Add(new BackupDeviceItem(
                   backupFileName, DeviceType.File));
                backup.SqlBackup(server);
            }
            finally
            {
                try
                {
                    // Verbindung zum SQL-Server abbauen
                    server.ConnectionContext.Disconnect();
                }
                catch { }
            }
        }

        /// <summary>
        /// Restauriert ein Backup einer Datenbank 
        /// </summary>
        /// <param name="serverName">Der Name des SQL Servers</param>
        /// <param name="databaseName">Der Name der wiederherzustellenden Datenbank</param>
        /// <param name="backupFileName">Der Name der Datei, die das Backup enthält</param>
        /// <param name="useTrustedConnection">Gibt an, ob eine vertraute Verbindung verwendet werden soll</param>
        /// <param name="login">Login-Name für den Login falls keine vertraute Verbindung verwendet werden soll</param>
        /// <param name="password">Passwort für den Login falls keine vertraute Verbindung verwendet werden soll</param>
        public void RestoreFromFile(string serverName, string databaseName,
        string backupFileName, bool useTrustedConnection, string login,
        string password)
        {
            // Verbindungsinformationen definieren
            ServerConnection serverConnection = new ServerConnection();
            serverConnection.ServerInstance = serverName;
            if (useTrustedConnection)
            {
                serverConnection.LoginSecure = true;
            }
            else
            {
                serverConnection.LoginSecure = false;
                serverConnection.Login = login;
                serverConnection.Password = password;
            }

            // Verbindung aufbauen
            Server server = new Server(serverConnection);
            try
            {
                server.ConnectionContext.Connect();

                // Restore aus der angegebenen Datei
                Restore restore = new Restore();
                restore.Action = RestoreActionType.Database;
                restore.Database = databaseName;
                if (this.PercentComplete != null)
                {
                    restore.PercentComplete += this.PercentComplete;
                }
                restore.Devices.Add(new BackupDeviceItem(
                   backupFileName, DeviceType.File));
                restore.SqlRestore(server);
            }
            finally
            {
                try
                {

                    // Verbindung zum SQL-Server abbauen
                    server.ConnectionContext.Disconnect();
                }
                catch { }
            }
        }
    }
}
