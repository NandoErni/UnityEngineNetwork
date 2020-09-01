using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngineNetwork {
  public abstract class BaseNetworkManager {
    public BaseNetworkManager() {
      InitPacketHandlers();
    }

    /// <summary>Initilizes all packet handlers.</summary>
    protected abstract void InitPacketHandlers();
  }
}
