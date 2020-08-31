using System;

namespace UnityEngineNetwork {
  public class ClientEventArgs : EventArgs {
    public Server.Client Client;

    public Protocol Protocol;

    public ClientEventArgs(Server.Client client, Protocol protocol) {
      Client = client;
      Protocol = protocol;
    }
  }

  public enum Protocol {
    TCP,
    UDP
  }
}
