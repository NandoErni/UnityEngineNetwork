namespace UnityEngineNetwork.Client {
  /// <summary>Manages the client to server communication</summary>
  public abstract class BaseClientNetworkManager : BaseNetworkManager {
    /// <summary>Manages the client to server communication</summary>
    public BaseClientNetworkManager() : base() {
    }
    /// <summary>The client singleton</summary>
    protected IClient ClientInstance => Client.Instance;
  }
}
