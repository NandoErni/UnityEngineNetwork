namespace UnityEngineNetwork.Server {
  /// <summary>Manages the server</summary>
  public abstract class BaseServerNetworkManager : BaseNetworkManager {

    /// <summary>Creates a new server network manager</summary>
    public BaseServerNetworkManager() : base() {

    }

    /// <summary>The server singleton</summary>
    protected IServer ServerInstance => Server.Instance;
  }
}
