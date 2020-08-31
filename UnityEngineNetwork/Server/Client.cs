using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngineNetwork.Server {
  public class Client {
    public int Id;
    public string Username;
    public TCP Tcp;
    public UDP Udp;

    public bool IsConnected => Tcp.Socket != null;

    public event OnClientDisconnectedEventHandler OnClientDisconnected;

    public Client(int clientId) {
      Id = clientId;
      Tcp = new TCP(Id);
      Udp = new UDP(Id);
    }

    public void Disconnect() {
      Tcp.Disconnect();
      Udp.Disconnect();

      OnClientDisconnected?.Invoke(this, new ClientEventArgs(this, Protocol.TCP & Protocol.UDP));
    }
  }
}
