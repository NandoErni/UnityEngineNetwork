using System;
using System.Collections.Generic;
using System.Text;
using UnityEngineNetwork;

namespace UnityEngineNetworkPlayground {
  public class Network : NetworkManager {
    public Network() {
      ClientInstance.ConnectToServer(new ServerRepo(), Constants.LocalHost, "usernamero");
      ((ServerRepo)ClientInstance.ServerRepository).SendYobama("");
    }
  }

  public class ServerRepo : UnityEngineNetwork.Client.ServerRepository {
    public void SendYobama(string yob) {
      using (Packet packet = new Packet(0)) {
        packet.Write(yob);

        SendTCPData(packet);
      }
    }
  }
}
