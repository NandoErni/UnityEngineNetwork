using System;
using System.Collections.Generic;
using System.Text;
using UnityEngineNetwork.Client;
using UnityEngineNetwork.Server;

namespace UnityEngineNetwork {
  public abstract class BaseClientNetworkManager {
    /// <summary>The client singleton</summary>
    protected IClient ClientInstance => Client.Client.Instance;

    public BaseClientNetworkManager() {
      InitPacketHandlers();
    }

    /// <summary>Initilizes all packet handlers.</summary>
    protected abstract void InitPacketHandlers();
  }
}
