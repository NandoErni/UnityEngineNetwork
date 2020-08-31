using System;
using System.Net.Sockets;

namespace UnityEngineNetwork.Server {

  public class TCP {
    public TcpClient Socket;

    private NetworkStream _networkStream;

    private Packet _receivedData;

    private byte[] _receiveBuffer;

    private readonly int _id;

    public TCP(int id) {
      _id = id;
    }

    public void Connect(TcpClient socket) {
      Socket = socket;
      Socket.ReceiveBufferSize = Constants.DataBufferSize;

      _networkStream = Socket.GetStream();

      _receivedData = new Packet();

      _receiveBuffer = new byte[Constants.DataBufferSize];

      _networkStream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);

      Server.Instance.ClientRepository.SendWelcome(_id, "Welcome to the Server");
    }

    public void SendData(Packet packet) {
      if (Socket != null) {
        _networkStream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
      }
    }

    private void ReceiveCallback(IAsyncResult result) {
      try {
        int byteLength = _networkStream.EndRead(result);
        if (byteLength <= 0) {
          Server.Instance.Clients[_id].Disconnect();
          return;
        }

        byte[] data = new byte[byteLength];
        Array.Copy(_receiveBuffer, data, byteLength);

        _receivedData.Reset(HandleData(data));

        _networkStream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);
      }
      catch (Exception ex) {
        Server.Instance.Clients[_id].Disconnect();
        throw ex;
      }
    }

    private bool HandleData(byte[] data) {
      int packetLength = 0;

      _receivedData.SetBytes(data);

      if (_receivedData.UnreadLength() >= 4) {
        packetLength = _receivedData.ReadInt();
        if (packetLength <= 0) {
          return true;
        }
      }

      while (packetLength > 0 && packetLength <= _receivedData.UnreadLength()) {
        byte[] packetBytes = _receivedData.ReadBytes(packetLength);
        Server.Instance.ExecuteOnMainThread(() => {
          using (Packet packet = new Packet(packetBytes)) {
            int packetId = packet.ReadInt();
            Server.Instance.PacketHandlers[packetId](_id, packet);
          }
        });

        packetLength = 0;
        if (_receivedData.UnreadLength() >= 4) {
          packetLength = _receivedData.ReadInt();
          if (packetLength <= 0) {
            return true;
          }
        }
      }

      if (packetLength <= 1) {
        return true;
      }

      return false;
    }

    public void Disconnect() {
      Socket.Close();
      _networkStream = null;
      _receiveBuffer = null;
      _receivedData = null;
      Socket = null;
    }
  }
}
