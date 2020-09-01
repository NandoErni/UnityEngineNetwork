namespace UnityEngineNetwork.Client {
  public abstract class BaseClientNetworkManager : BaseNetworkManager {
    public BaseClientNetworkManager() : base() {
    }
    /// <summary>The client singleton</summary>
    protected IClient ClientInstance => Client.Instance;
  }
}
