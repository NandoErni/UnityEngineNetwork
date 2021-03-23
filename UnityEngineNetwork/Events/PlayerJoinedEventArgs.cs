using System;

namespace UnityEngineNetwork {

  /// <summary>Eventargs for dealing with clients and its protocols.</summary>
  public class ClientEventArgs : EventArgs {

    /// <summary>The client</summary>
    public Server.Client Client;

    /// <summary>The protocol which was used</summary>
    public Protocol Protocol;

    /// <summary>Create eventargs with a client and its protocol</summary>
    /// <param name="client"></param>
    /// <param name="protocol"></param>
    public ClientEventArgs(Server.Client client, Protocol protocol) {
      Client = client;
      Protocol = protocol;
    }
  }
}
