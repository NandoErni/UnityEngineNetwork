using System;
using System.Net;
using System.Net.Sockets;

namespace UnityEngineNetwork.Client {

  /// <summary>Class to handle all UDP requests.</summary>
  internal class UDP {
    private IPEndPoint _endPoint;

    private UdpClient _socket;

    private DataHandler _dataHandler;

    private int _clientId;

    /// <summary>Gets invoked when a connection to the server has been established.</summary>
    public event OnDisconnectEventHandler OnDisconnect;

    /// <summary>Creates a new UDP object and parses the given ip address and port</summary>
    /// <param name="ipAddress">The ip address</param>
    /// <param name="port">The port</param>
    public UDP(string ipAddress, int port, DataHandler dataHandler) {
      _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
      _dataHandler = dataHandler;
    }

    /// <summary>Connects to the server through a certain port.</summary>
    /// <param name="localPort">The local port</param>
    public void Connect(int localPort, int clientId) {
      _socket = new UdpClient(localPort);

      _socket.Connect(_endPoint);
      _socket.BeginReceive(ReceiveCallback, null);

      _clientId = clientId;

      using (Packet packet = new Packet()) {
        SendData(packet);
      }
    }

    /// <summary>Sends the given packet to the server.</summary>
    /// <param name="packet">The packet</param>
    public void SendData(Packet packet) {
      packet.InsertInt(_clientId);

      if (_socket != null) {
        _socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
      }
    }

    private void ReceiveCallback(IAsyncResult result) {
      try {
        byte[] data = _socket.EndReceive(result, ref _endPoint);
        _socket.BeginReceive(ReceiveCallback, null);

        if (data.Length < 4) {
          OnDisconnect?.Invoke(this, new EventArgs());
          return;
        }
        HandleData(data);
      }
      catch (Exception) {
        OnDisconnect?.Invoke(this, new EventArgs());
      }
    }

    private void HandleData(byte[] data) {
      using (Packet packet = new Packet(data)) {
        int packetLength = packet.ReadInt();
        data = packet.ReadBytes(packetLength);
      }

      _dataHandler.Handle(data);
    }

    /// <summary>Disconnects from the server.</summary>
    public void Disconnect() {
      if (_socket != null) {
        _socket.Close();
      }
      _socket = null;
    }
  }
}
