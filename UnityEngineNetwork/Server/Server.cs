using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace UnityEngineNetwork.Server {
  /// <inheritdoc/>
  public sealed class Server : IServer {
    #region Static
    /// <inheritdoc/>
    public static Server Instance => _instance;

    private static readonly Server _instance = new Server();
    #endregion

    #region Variables
    /// <inheritdoc/>
    public BaseClientRepository ClientRepository { get; private set; }

    /// <inheritdoc/>
    public Dictionary<int, Client> Clients { get; private set; } = new Dictionary<int, Client>();

    /// <inheritdoc/>
    public int MaxClients { get; private set; }

    /// <inheritdoc/>
    public int Port { get; private set; }

    private TcpListener _tcpListener;

    internal PacketHandler<PacketHandlerDelegate> PacketHandler = new PacketHandler<PacketHandlerDelegate>();

    internal UdpClient UdpListener { get; private set; }

    /// <inheritdoc/>
    public bool IsRunning { get; private set; }

    private List<string> _stringBlockList = new List<string>();

    internal ThreadManager ThreadManager = new ThreadManager();
    #endregion

    #region Delegates
    /// <inheritdoc/>
    public delegate void PacketHandlerDelegate(int clientId, Packet packet);
    #endregion

    #region Events
    /// <inheritdoc/>
    public event OnClientConnectedEventHandler OnClientConnected;

    /// <inheritdoc/>
    public event OnClientDisconnectedEventHandler OnClientDisconnected;

    /// <inheritdoc/>
    public event OnServerStartedEventHandler OnServerStarted;

    /// <inheritdoc/>
    public event OnServerStoppedEventHandler OnServerStopped;
    #endregion

    #region Constructor
    static Server() { }

    private Server() {
    }
    #endregion

    #region Init
    private void InitServerData() {
      for (int i = 0; i < MaxClients; i++) {
        Clients.Add(i, new Client(i));
        Clients[i].OnClientDisconnected += (sender, e) => { OnClientDisconnected?.Invoke(this, e); };
      }
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public void Start(BaseClientRepository clientRepository, int maxClients, int port = Constants.DefaultPort) {
      ClientRepository = clientRepository;
      MaxClients = maxClients;
      Port = port;

      PacketHandler.AddPacketHandler(0, clientRepository.HandleWelcomeReceived);
      InitServerData();

      _tcpListener = new TcpListener(IPAddress.Any, Port);
      _tcpListener.Start();
      _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

      UdpListener = new UdpClient(Port);
      UdpListener.BeginReceive(UDPReceiveCallback, null);

      IsRunning = true;

      OnServerStarted?.Invoke(this, new EventArgs());
    }

    /// <summary>Stops the server</summary>
    public void Stop() {
      Clients = new Dictionary<int, Client>();
      _tcpListener.Stop();
      UdpListener.Close();
      IsRunning = false;
      OnServerStopped?.Invoke(this, new EventArgs());
    }

    /// <inheritdoc/>
    public void AddPacketHandler(int id, PacketHandlerDelegate handler) {
      PacketHandler.AddPacketHandler(id, handler);
    }

    /// <inheritdoc/>
    public int GetRandomClientId(bool hasToBeConnected = true) {
      int clientId;
      Random random = new Random();

      if (hasToBeConnected) {
        List<int> ids = Clients.Where(x => x.Value.IsConnected).Select(x => x.Value.Id).ToList();

        clientId = random.Next(0, ids.Count);
      }
      else {
        clientId = random.Next(0, MaxClients);
      }

      return clientId;
    }

    /// <inheritdoc/>
    public string FilterOutBlockedStrings(string message, char replacementChar = '*') {
      foreach (var offensiveWord in _stringBlockList) {
        if (message.Contains(offensiveWord)) {
          String replacement = "";

          for (int i = 0; i < offensiveWord.Length; i++) {
            replacement += replacementChar;
          }

          message = message.Replace(offensiveWord, replacement);
        }
      }

      return message;
    }

    /// <inheritdoc/>
    public void AddStringToBlock(string stringToBlock) {
      _stringBlockList.Add(stringToBlock);
    }
    #endregion

    #region Callbacks
    private void TCPConnectCallback(IAsyncResult result) {
      TcpClient client = _tcpListener.EndAcceptTcpClient(result);
      _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

      for (int i = 0; i < MaxClients; i++) {
        if (Clients[i].Tcp.Socket == null) {
          Clients[i].Tcp.Connect(client);
          OnClientConnected?.Invoke(this, new ClientEventArgs(Clients[i], Protocol.TCP));
          return;
        }
      }

      throw new ConnectionFailedException($"{client.Client.RemoteEndPoint}: Server is full."); // probably not good
    }

    private void UDPReceiveCallback(IAsyncResult result) {
      IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
      byte[] data = UdpListener.EndReceive(result, ref clientEndPoint);
      UdpListener.BeginReceive(UDPReceiveCallback, null);

      if (data.Length < 4) {
        return;
      }

      using (Packet packet = new Packet(data)) {
        int clientId = packet.ReadInt();

        if (clientId == 0) {
          return;
        }

        if (Clients[clientId].Udp.EndPoint == null) {
          Clients[clientId].Udp.Connect(clientEndPoint);
          OnClientConnected?.Invoke(this, new ClientEventArgs(Clients[clientId], Protocol.UDP));
          return;
        }

        if (Clients[clientId].Udp.EndPoint.ToString() == clientEndPoint.ToString()) {
          Clients[clientId].Udp.HandleData(packet);
        }
      }
    }
    #endregion

    #region Managing Thread

    /// <inheritdoc/>
    public void UpdateMain() {
      ThreadManager.UpdateMain();
    }
    #endregion
  }
}
