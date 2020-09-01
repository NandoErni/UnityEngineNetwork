namespace UnityEngineNetwork.Server {
  public abstract class BaseServerNetworkManager {
    /// <summary>The server singleton</summary>
    protected IServer ServerInstance => Server.Instance;

    public BaseServerNetworkManager() {
      InitPacketHandlers();
    }

    /// <summary>Initilizes all packet handlers.</summary>
    protected abstract void InitPacketHandlers();
  }
}
