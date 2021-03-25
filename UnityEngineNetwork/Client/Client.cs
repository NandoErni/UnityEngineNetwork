using System;
using System.Net;

namespace UnityEngineNetwork.Client {
  /// <inheritdoc/>
  public sealed class Client : IClient {
    #region Static
    /// <summary>The singleton instance</summary>
    public static Client Instance => _instance;

    private static readonly Client _instance = new Client();
    #endregion

    #region Variables & Properties
    /// <inheritdoc/>
    public int Id { get; private set; } = 0;

    /// <inheritdoc/>
    public string ServerIpAddress { get; private set; }

    /// <inheritdoc/>
    public int Port { get; private set; }

    /// <inheritdoc/>
    public string Username { get; private set; }

    /// <inheritdoc/>
    public bool IsConnected { get; set; } = false;

    /// <inheritdoc/>
    public BaseServerRepository ServerRepository { get; private set; }

    internal ThreadManager ThreadManager = new ThreadManager();

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
    public event OnConnectEventHandler OnConnect;
    #endregion

    #region Constructor
    static Client() { }

    private Client() { }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public void ReconnectToServer() {      
      _tcp.Connect();
    }

    /// <inheritdoc/>
    public void ConnectToServer(BaseServerRepository repository, string username, string ipAddress = Constants.LocalHost, int port = Constants.DefaultPort) {
      if (String.IsNullOrEmpty(username.Trim())) {
        throw new ArgumentException($"The username '{username}' is not valid");
      }
      if (String.IsNullOrEmpty(ipAddress.Trim())) {
        throw new ArgumentException($"The ip address '{ipAddress}' is not valid");
      }
      if (port <= 0) {
        throw new ArgumentException($"The port '{port}' is not valid");
      }
      if (repository == null) {
        throw new ArgumentException("The repository must not be null");
      }

      ServerRepository = repository;
      AddPacketHandler(0, ServerRepository.HandleWelcome);
      ServerIpAddress = ipAddress;
      Port = port;
      Username = username;
      _tcp = new TCP();
      _udp = new UDP(ServerIpAddress, Port);
      _tcp.OnConnect += (sender, e) => { OnConnect?.Invoke(this, e); };
      ReconnectToServer();
    }

    internal void ConnectToUdp() {
      _udp.Connect(((IPEndPoint)_tcp?.Socket?.Client.LocalEndPoint).Port);
    }

    internal void InitId(int id) {
      Id = id;
    }

    /// <inheritdoc/>
    internal void SendTCPData(Packet packet) {
      _tcp.SendData(packet);
    }


    /// <inheritdoc/>
    internal void SendUDPData(Packet packet) {
      _udp.SendData(packet);
    }

    internal void HandleReceivedPacket(Packet packet) {
      _packetHandler.Get(packet.ReadInt())(packet);
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
      ThreadManager.UpdateMain();
    }
    #endregion
  }
}
