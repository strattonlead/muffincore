namespace Muffin.StateManagement
{
    public class AppStateEvents<TAppState>
    {
        public AppStateEvent<TAppState> OnAppStateChangedEvent;
    }

    public delegate void AppStateEvent<TAppState>(object sender, AppStateEventArgs<TAppState> args);

    public class AppStateEventArgs<TAppState>
    {
        #region Properties

        public long? CredentialId { get; set; }
        public TAppState AppState { get; set; }
        public PartialAppState PartialAppState { get; set; }

        #endregion

        #region Constructor

        public AppStateEventArgs() { }

        public AppStateEventArgs(long? credentialId)
        {
            CredentialId = credentialId;
        }

        public AppStateEventArgs(long? credentialId, TAppState appState)
        {
            CredentialId = credentialId;
            AppState = appState;
        }

        public AppStateEventArgs(long? credentialId, PartialAppState partialAppState)
        {
            CredentialId = credentialId;
            PartialAppState = partialAppState;
        }

        public AppStateEventArgs(long? credentialId, TAppState appState, PartialAppState partialAppState)
        {
            CredentialId = credentialId;
            AppState = appState;
            PartialAppState = partialAppState;
        }

        #endregion
    }
}
