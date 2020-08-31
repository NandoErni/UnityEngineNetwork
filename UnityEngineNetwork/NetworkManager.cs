using System;
using System.Collections.Generic;
using System.Text;
using UnityEngineNetwork.Client;
using UnityEngineNetwork.Server;

namespace UnityEngineNetwork {
  public abstract class NetworkManager {
    public IClient ClientInstance => Client.Client.Instance;
    public IServer ServerInstance => Server.Server.Instance;

    public NetworkManager() {
      InitPacketHandlers();
    }

    public abstract void InitPacketHandlers();
  }
}
