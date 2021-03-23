namespace UnityEngineNetwork.Server {
  /// <summary>Represents a client</summary>
  public class Client {
    /// <summary>The client id</summary>
    public int Id;

    /// <summary>The username of the client</summary>
    public string Username;

    internal TCP Tcp;
    
    internal UDP Udp;

    /// <summary>Indictes whether the client is connected to the server</summary>
    public bool IsConnected => Tcp.Socket != null;

    /// <summary>Gets invoked when a client disconnects</summary>
    public event OnClientDisconnectedEventHandler OnClientDisconnected;

    /// <summary>Creates a new client with its clientid</summary>
    /// <param name="clientId"></param>
    public Client(int clientId) {
      Id = clientId;
      Tcp = new TCP(Id);
      Udp = new UDP(Id);
    }

    /// <summary>Disconnects the client from the server</summary>
    public void Disconnect() {
      Tcp.Disconnect();
      Udp.Disconnect();

      OnClientDisconnected?.Invoke(this, new ClientEventArgs(this, Protocol.TCP & Protocol.UDP));
    }
  }
}
