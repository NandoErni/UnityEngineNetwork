namespace UnityEngineNetwork.Server {
  public abstract class BaseServerNetworkManager : BaseNetworkManager {
    public BaseServerNetworkManager() : base() {

    }

    /// <summary>The server singleton</summary>
    protected IServer ServerInstance => Server.Instance;
  }
}
