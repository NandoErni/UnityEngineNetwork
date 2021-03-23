namespace UnityEngineNetwork.Client {
  /// <summary>Repository for all communication between client and server</summary>
  public abstract class BaseServerRepository {

    /// <summary>Gets called when the client receives the welcome message from the server.</summary>
    public event OnWelcomeReceivedEventHandler OnWelcomeReceived;

    /// <summary>Sends the specified packet to the Server using TCP.</summary>
    /// <param name="packet">The packet</param>
    protected void SendTCPData(Packet packet) {
      packet.WriteLength();
      Client.Instance.SendTCPData(packet);
    }

    /// <summary>Sends the specified packet to the Server using UDP.</summary>
    /// <param name="packet">The packet</param>
    protected void SendUDPData(Packet packet) {
      packet.WriteLength();
      Client.Instance.SendUDPData(packet);
    }

    /// <summary>Handles the welcome message from the server.</summary>
    /// <param name="packet">the packet</param>
    public void HandleWelcome(Packet packet) {
      string message = packet.ReadString();
      int id = packet.ReadInt();

      Client.Instance.InitId(id);
      SendWelcomeReceived();

      Client.Instance.ConnectToUdp();

      OnWelcomeReceived?.Invoke(this, new WelcomeEventArgs(message));
    }

    /// <summary>Sends a packet to the server containing the username.</summary>
    public void SendWelcomeReceived() {
      using (Packet packet = new Packet(0)) {
        packet.Write(Client.Instance.Id);
        packet.Write(Client.Instance.Username);

        SendTCPData(packet);
      }
    }
  }
}
