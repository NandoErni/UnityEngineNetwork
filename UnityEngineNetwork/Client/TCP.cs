using System;
using System.Net.Sockets;

namespace UnityEngineNetwork.Client {
  /// <summary>A class to handle all TCP requests.</summary>
  internal class TCP {
    /// <summary>The tcp client</summary>
    public TcpClient Socket { get; private set; }

    private NetworkStream _networkStream;

    private Packet _receivedData;

    private byte[] _receiveBuffer;

    private string _serverIpAddress;

    private int _serverPort;

    private DataHandler _dataHandler;

    /// <summary>Gets invoked when a connection to the server has been established.</summary>
    public event OnConnectEventHandler OnConnect;

    /// <summary>Gets invoked when a connection to the server has been established.</summary>
    public event OnDisconnectEventHandler OnDisconnect;

    public TCP(string ipAddress, int port, DataHandler dataHandler) {
      _serverIpAddress = ipAddress;
      _serverPort = port;
      _dataHandler = dataHandler;
    }

    /// <summary>Connects to the server.</summary>
    public void Connect() {
      Socket = new TcpClient {
        ReceiveBufferSize = Constants.DataBufferSize,
        SendBufferSize = Constants.DataBufferSize,
      };

      _receiveBuffer = new byte[Constants.DataBufferSize];

      Socket.BeginConnect(_serverIpAddress, _serverPort, ConnectCallback, Socket);
    }

    private void ConnectCallback(IAsyncResult result) {
      if (Socket.Connected) {
        Socket.EndConnect(result);
      }
      else {
        throw new ConnectionFailedException($"Unable to connect to Server {_serverIpAddress}.");
      }

      OnConnect?.Invoke(this, new EventArgs());

      _networkStream = Socket.GetStream();

      _receivedData = new Packet();

      _networkStream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult result) {
      try {
        int byteLength = _networkStream.EndRead(result);
        if (byteLength <= 0) {
          OnDisconnect?.Invoke(this, new EventArgs());
          return;
        }

        byte[] data = new byte[byteLength];
        Array.Copy(_receiveBuffer, data, byteLength);

        _receivedData.Reset(HandleData(data));

        _networkStream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);
      }
      catch (Exception ex) {
        OnDisconnect?.Invoke(this, new EventArgs());
      }
    }

    /// <summary>Sends a packet to the server.</summary>
    /// <param name="packet">The packet</param>
    public void SendData(Packet packet) {
      if (Socket != null) {
        _networkStream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
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
        _dataHandler.Handle(packetBytes);

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

    /// <summary>Disconnects from the server.</summary>
    public void Disconnect() {
      Socket.Close();
      if (_networkStream != null) {
        _networkStream.Close();
      }
      _networkStream = null;
      _receiveBuffer = null;
      _receivedData = null;
      Socket = null;
    }
  }
}
