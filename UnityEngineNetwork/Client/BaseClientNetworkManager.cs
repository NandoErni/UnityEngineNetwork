namespace UnityEngineNetwork.Client {
  public abstract class BaseClientNetworkManager {
    /// <summary>The client singleton</summary>
    protected IClient ClientInstance => Client.Instance;

    public BaseClientNetworkManager() {
      InitPacketHandlers();
    }

    /// <summary>Initilizes all packet handlers.</summary>
    protected abstract void InitPacketHandlers();
  }
}
