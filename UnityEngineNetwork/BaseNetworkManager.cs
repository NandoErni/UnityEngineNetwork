namespace UnityEngineNetwork {
  /// <summary>Represents a network manager</summary>
  public abstract class BaseNetworkManager {

    /// <summary>Creates a new network manager and initilizes the packet handlers</summary>
    public BaseNetworkManager() {
      InitPacketHandlers();
    }

    /// <summary>Initilizes all packet handlers.</summary>
    protected abstract void InitPacketHandlers();
  }
}
