using System;
using System.Net.Sockets;

namespace UnityEngineNetwork.Server {

  /// <summary>Class for all TCP requests</summary>
  internal class TCP {
    /// <summary>The tcp client</summary>
    public TcpClient Socket { get; private set; }

    private NetworkStream _networkStream;

    private Packet _receivedData;

    private byte[] _receiveBuffer;

    private readonly int _clientId;

    /// <summary>Creates a tcp class with a client id</summary>
    /// <param name="clientId">The client id</param>
    public TCP(int clientId) {
      _clientId = clientId;
    }

    /// <summary>Connects to the given client</summary>
    /// <param name="socket">The client</param>
    public void Connect(TcpClient socket) {
      Socket = socket;
      Socket.ReceiveBufferSize = Constants.DataBufferSize;

      _networkStream = Socket.GetStream();

      _receivedData = new Packet();

      _receiveBuffer = new byte[Constants.DataBufferSize];

      _networkStream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);

      Server.Instance.ClientRepository.SendWelcome(_clientId, "Welcome to the Server");
    }

    /// <summary>Sends a packet to the client </summary>
    /// <param name="packet"></param>
    public void SendData(Packet packet) {
      if (Socket != null) {
        _networkStream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
      }
    }

    private void ReceiveCallback(IAsyncResult result) {
      try {
        int byteLength = _networkStream.EndRead(result);
        if (byteLength <= 0) {
          Server.Instance.Clients[_clientId].Disconnect();
          return;
        }

        byte[] data = new byte[byteLength];
        Array.Copy(_receiveBuffer, data, byteLength);

        _receivedData.Reset(HandleData(data));

        _networkStream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);
      }
      catch (Exception ex) {
        Server.Instance.Clients[_clientId].Disconnect();
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
        Server.Instance.ThreadManager.ExecuteOnMainThread(() => {
          using (Packet packet = new Packet(packetBytes)) {
            int packetId = packet.ReadInt();
            Server.Instance.PacketHandler.Get(packetId)(_clientId, packet);
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

    /// <summary>Disconnects from the client.</summary>
    public void Disconnect() {
      Socket.Close();
      _networkStream = null;
      _receiveBuffer = null;
      _receivedData = null;
      Socket = null;
    }
  }
}
