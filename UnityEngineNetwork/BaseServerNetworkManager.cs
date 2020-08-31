using System;
using System.Collections.Generic;
using System.Text;
using UnityEngineNetwork.Client;
using UnityEngineNetwork.Server;

namespace UnityEngineNetwork {
  public abstract class BaseServerNetworkManager {
    /// <summary>The server singleton</summary>
    protected IServer ServerInstance => Server.Server.Instance;

    public BaseServerNetworkManager() {
      InitPacketHandlers();
    }

    /// <summary>Initilizes all packet handlers.</summary>
    protected abstract void InitPacketHandlers();
  }
}
