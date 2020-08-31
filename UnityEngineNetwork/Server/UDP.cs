using System.Net;

namespace UnityEngineNetwork.Server {

  public class UDP {
    public IPEndPoint EndPoint;

    private int _id;

    public UDP(int id) {
      _id = id;
    }

    public void Connect(IPEndPoint endPoint) {
      EndPoint = endPoint;
    }

    public void SendData(Packet packet) {
      if (EndPoint != null) {
        Server.Instance.UdpListener.BeginSend(packet.ToArray(), packet.Length(), EndPoint, null, null);
      }
    }

    public void HanldeData(Packet data) {
      int packetLength = data.ReadInt();
      byte[] packetBytes = data.ReadBytes(packetLength);

      Server.Instance.ExecuteOnMainThread(() => {
        using (Packet packet = new Packet(packetBytes)) {
          int packetId = packet.ReadInt();
          Server.Instance.PacketHandlers[packetId](_id, packet);
        }
      });
    }

    public void Disconnect() {
      EndPoint = null;
    }
  }
}
