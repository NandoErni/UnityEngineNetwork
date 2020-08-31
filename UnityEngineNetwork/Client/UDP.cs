using System;
using System.Net;
using System.Net.Sockets;

namespace UnityEngineNetwork.Client {
  public class UDP {
    public UdpClient Socket;
    public IPEndPoint EndPoint;

    public UDP() {
      EndPoint = new IPEndPoint(IPAddress.Parse(Client.Instance.ServerIpAddress), Client.Instance.Port);
    }

    public void Connect(int localPort) {
      Socket = new UdpClient(localPort);

      Socket.Connect(EndPoint);
      Socket.BeginReceive(ReceiveCallback, null);

      using (Packet packet = new Packet()) {
        SendData(packet);
      }
    }

    public void SendData(Packet packet) {
      packet.InsertInt(Client.Instance.Id);

      if (Socket != null) {
        Socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
      }
    }

    private void ReceiveCallback(IAsyncResult result) {
      try {
        byte[] data = Socket.EndReceive(result, ref EndPoint);
        Socket.BeginReceive(ReceiveCallback, null);

        if (data.Length < 4) {
          Client.Instance.Disconnect();
          return;
        }
        HandleData(data);
      }
      catch (Exception) {
        Client.Instance.Disconnect();
      }
    }

    private void HandleData(byte[] data) {
      using (Packet packet = new Packet(data)) {
        int packetLength = packet.ReadInt();
        data = packet.ReadBytes(packetLength);
      }

      Client.Instance.ExecuteOnMainThread(() => {
        using (Packet packet = new Packet(data)) {
          int packetId = packet.ReadInt();
          Client.Instance.HandleReceivedPacket(packet);
        }
      });
    }

    public void Disconnect() {
      if (Socket != null) {
        Socket.Close();
      }
      EndPoint = null;
      Socket = null;
    }
  }
}
