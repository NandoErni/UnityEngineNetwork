using System.Net;

namespace UnityEngineNetwork.Server {

  /// <summary>Class to handle all UDP requests.</summary>
  internal class UDP {
    /// <summary>The IP endpoint of the client.</summary>
    public IPEndPoint EndPoint { get; private set; }

    private int _id;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public UDP(int id) {
      _id = id;
    }

    /// <summary>Connects to the Client.</summary>
    /// <param name="endPoint">The endpoint to connect to</param>
    public void Connect(IPEndPoint endPoint) {
      EndPoint = endPoint;
    }

    /// <summary>Sends a packet to the client.</summary>
    /// <param name="packet">The packet to send.</param>
    public void SendData(Packet packet) {
      if (EndPoint != null) {
        Server.Instance.UdpListener.BeginSend(packet.ToArray(), packet.Length(), EndPoint, null, null);
      }
    }

    /// <summary>Handles a received packet.</summary>
    /// <param name="data">The packet</param>
    public void HandleData(Packet data) {
      int packetLength = data.ReadInt();
      byte[] packetBytes = data.ReadBytes(packetLength);

      Server.Instance.ThreadManager.ExecuteOnMainThread(() => {
        using (Packet packet = new Packet(packetBytes)) {
          int packetId = packet.ReadInt();
          Server.Instance.PacketHandler.Get(packetId)(_id, packet);
        }
      });
    }

    /// <summary>Disconnects from the Client.</summary>
    public void Disconnect() {
      EndPoint = null;
    }
  }
}
