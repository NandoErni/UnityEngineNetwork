using System;
using System.Net;

namespace UnityEngineNetwork.Client {
  /// <inheritdoc/>
  public sealed class Client : IClient {

    #region Variables & Properties
    /// <inheritdoc/>
    public int Id { get; private set; } = 0;

    /// <inheritdoc/>
    public string ServerIpAddress { get; private set; }

    /// <inheritdoc/>
    public int ServerPort { get; private set; }

    /// <inheritdoc/>
    public string Username { get; private set; }

    /// <inheritdoc/>
    public bool IsConnected { get; private set; } = false;

    private ThreadManager _threadManager = new ThreadManager();

    private TCP _tcp;

    private UDP _udp;

    private PacketHandler<PacketHandlerDelegate> _packetHandler = new PacketHandler<PacketHandlerDelegate>();

    /// <inheritdoc/>
    public delegate void PacketHandlerDelegate(Packet packet);
    #endregion

    #region Events
    /// <inheritdoc/>
    public event OnDisconnectEventHandler OnDisconnect;

    /// <inheritdoc/>
    public event OnWelcomeReceivedEventHandler OnWelcomeReceived;
    #endregion

    #region Constructor
    /// <summary>Creates a client. The client is not connected yet.</summary>
    /// <param name="ipAddress">The IP address of the server.</param>
    /// <param name="port">The port of the server.</param>
    /// <param name="username">The username of this client</param>
    public Client(string username, string ipAddress = Constants.LocalHost, int port = Constants.DefaultPort) {
      if (string.IsNullOrEmpty(username.Trim())) {
        throw new ArgumentException($"The username '{username}' is not valid");
      }
      if (string.IsNullOrEmpty(ipAddress.Trim())) {
        throw new ArgumentException($"The ip address '{ipAddress}' is not valid");
      }
      if (port <= 0) {
        throw new ArgumentException($"The port '{port}' is not valid");
      }

      _packetHandler.TryAddPackerHandler(0, HandleWelcome);

      ServerIpAddress = ipAddress;
      ServerPort = port;
      Username = username;

      DataHandler _dataHandler = new DataHandler(HandleReceivedPacket);

      _tcp = new TCP(ServerIpAddress, ServerPort, _dataHandler);
      _tcp.OnConnect += (s, e) => IsConnected = true;
      _tcp.OnDisconnect += (s, e) => Disconnect();

      _udp = new UDP(ServerIpAddress, ServerPort, _dataHandler);
      _udp.OnDisconnect += (s, e) => Disconnect();
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public void ConnectToServer() {
      _tcp.Connect();
    }

    private void ConnectToUdp(int clientId) {
      Id = clientId;
      _udp.Connect(((IPEndPoint)_tcp?.Socket?.Client.LocalEndPoint).Port, Id);
    }

    /// <inheritdoc/>
    public void SendTCPData(Packet packet) {
      if (!IsConnected) {
        throw new ConnectionFailedException("The client is not connected to the server!");
      }
      packet.WriteLength();
      _tcp.SendData(packet);
    }

    /// <inheritdoc/>
    public void SendUDPData(Packet packet) {
      if (!IsConnected) {
        throw new ConnectionFailedException("The client is not connected to the server!");
      }
      packet.WriteLength();
      _udp.SendData(packet);
    }

    private void HandleReceivedPacket(byte[] data) {
      _threadManager.ExecuteOnMainThread(() => {
        using (Packet packet = new Packet(data)) {
          if (packet == null) {
            return;
          }

          PacketHandlerDelegate handler = _packetHandler.Get(packet.ReadInt());

          if (handler != null) {
            handler(packet);
          }
        }
      });
    }

    /// <inheritdoc/>
    public void Disconnect() {
      if (IsConnected) {
        IsConnected = false;
        _tcp.Disconnect();
        _udp.Disconnect();

        OnDisconnect?.Invoke(this, new EventArgs());
      }
    }

    /// <inheritdoc/>
    public void AddPacketHandler(int id, PacketHandlerDelegate handler) {
      _packetHandler.AddPacketHandler(id, handler);
    }

    /// <inheritdoc/>
    public void UpdateMain() {
      _threadManager.UpdateMain();
    }

    /// <summary>Handles the welcome message from the server.</summary>
    /// <param name="packet">the packet</param>
    private void HandleWelcome(Packet packet) {
      string message = packet.ReadString();
      int id = packet.ReadInt();

      ConnectToUdp(id);
      OnWelcomeReceived?.Invoke(this, new WelcomeEventArgs(message, id));
      SendWelcomeReceived();
    }

    /// <summary>Sends a packet to the server containing the username.</summary>
    private void SendWelcomeReceived() {
      using (Packet packet = new Packet(0)) {
        packet.Write(Id);
        packet.Write(Username);

        SendTCPData(packet);
      }
    }
    #endregion
  }
}
