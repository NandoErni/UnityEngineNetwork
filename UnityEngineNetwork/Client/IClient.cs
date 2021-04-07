using System;

namespace UnityEngineNetwork.Client {
  /// <summary>The interface for the client singleton. You should never access it directly.</summary>
  public interface IClient {
    #region Properties
    /// <summary>The Player id. can be used to determine which player number the current client is.</summary>
    int Id { get; }

    /// <summary>The server ip address</summary>
    string ServerIpAddress { get; }

    /// <summary>The server port</summary>
    int ServerPort { get; }

    /// <summary>The username</summary>
    string Username { get; }

    /// <summary>Indicates whether the client is connected or not.</summary>
    bool IsConnected { get; }
    #endregion

    #region Methods
    /// <summary>Connects to the Server.</summary>
    /// <exception cref="ConnectionFailedException">Gets called if the connection fails.</exception>
    void ConnectToServer();

    /// <summary>Adds a packet handler. The packet handler is used to handle incomming packets from the server. 
    /// The id cannot be 0, because 0 is already used to send username and client id.</summary>
    /// <param name="id">The id of the packet handler. This id must match with the sender packet id of the server.</param>
    /// <param name="handler">The handler method</param>
    /// <exception cref="ArgumentException">Is thrown when the id is below 1</exception>
    void AddPacketHandler(int id, Client.PacketHandlerDelegate handler);

    /// <summary>Disconnects all connections.</summary>
    void Disconnect();

    /// <summary>Sends the packet to the server via TCP.</summary>
    /// <param name="packet">the packet</param>
    void SendTCPData(Packet packet);

    /// <summary>Sends the packet to the server via UDP.</summary>
    /// <param name="packet">The packet</param>
    void SendUDPData(Packet packet);

    /// <summary>Call this method inside of Unitys Update() method.</summary>
    void UpdateMain();
    #endregion

    #region Events
    /// <summary>Gets called when the client disconnects all connections.</summary>
    event OnDisconnectEventHandler OnDisconnect;

    /// <summary>Gets called when the client receives the welcome message from the server.</summary>
    event OnWelcomeReceivedEventHandler OnWelcomeReceived;
    #endregion
  }
}
