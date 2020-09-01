
# UnityEngineNetwork
UnityEngineNetwork is a C# Library for Unity to implement networking functionalities into a game. This library is split into to parts. The `UnityEngineNetwork.Server` namespace and the `UnityEngineNetwork.Client` namespace.
## Get started
### Create a client
#### Namespaces:
 - `UnityEngineNetwork` 
 - `UnityEngineNetwork.Client`

#### Steps:
 - Create a new ClientNetworkManager
 - Create a new ServerRepository
 - Connect to the server
 - Send packets
 - Handle packets
 
#### Create a new ClientNetworkManager
This library provides a ClientNetworkManager base class, which is used to manage everything done to the client. To use the ClientNetworkManager base class, create a new class, which derives from `UnityEngineNetwork.Client.BaseClientNetworkManager`

Example:

    public class ClientNetworkManager : UnityEngineNetwork.Client.BaseClientNetworkManager

**Note:**
A repository property can become handy:
`private ServerRepository _repository => ClientInstance.ServerRepository as ServerRepository;`

#### Create a new ServerRepository
Communication to and from the server should always run in the ServerRepository. To create your own, you will have to create a class which derives from `UnityEngineNetwork.Client.BaseServerRepository`.

Example:

    public class ServerRepository : UnityEngineNetwork.Client.BaseServerRepository
    
#### Connect to the server
To connect to the server just call `UnityEngineNetwork.Client.BaseClientNetworkManager.ClientInstance.ConnectToServer(...)`.
Do this in your own ClientNetworkManager.

#### Send packets
To send packets you have to create a method in the ServerRepository. In this method you have to call the method 
`UnityEngineNetwork.Client.BaseServerRepository.SendTCPData(Packet packet)` 
or 
`UnityEngineNetwork.Client.BaseServerRepository.SendUDPData(Packet packet)` 

Example:

    public void SendNumbers() {
	    using (Packet packet = new Packet(1)) {
	      packet.Write(3);
	      packet.Write(5);

	      SendTCPData(packet);
	    }
    }
##### Note: the packet id must be greater than 0. It also has to match with the packet id of the handler method of the server.

#### Handle packets
To handle any incoming packets, you have to create a new handler method in your ServerRepository. It has to match the delegate `UnityEngineNetwork.Client.Client.PacketHandler`

After you created the handler method, you have to add it to the packethandler. To do this, call the
`ClientInstance.AddPacketHandler(int id, Client.PacketHandler handler)` 
method in your ClientNetworkManager inside of 

    UnityEngineNetwork.Client.BaseClientNetworkManager.InitPacketHandlers()

Example:
##### HandlerMethod:
    public void HandleNumbers(Packet packet) {
      int num1 = packet.ReadInt();
      int num2 = packet.ReadInt();

      Console.WriteLine($"The sum is: {num1 + num2}!");
    }
##### Adding packet handler:
    protected override void InitPacketHandlers() {
      ClientInstance.AddPacketHandler(1, HandleNumbers);
    }
##### Note: the packet id must be greater than 0. It also has to match with the packet id of the send method of the server.

### Create the server
#### Namespaces:
 - `UnityEngineNetwork` 
 - `UnityEngineNetwork.Server`

#### Steps:
 - Create a new ServerNetworkManager
 - Create a new ClientRepository
 - Start the server
 - Send packets
 - Handle packets
 - 
#### Create a new ServerNetworkManager
This library provides a ServerNetworkManager base class, which is used to manage everything done to the server. To use the ServerNetworkManager base class, create a new class, which derives from `UnityEngineNetwork.Server.BaseServerNetworkManager`

Example:

    public class ServerNetworkManager : UnityEngineNetwork.Server.BaseServerNetworkManager

**Note:**
A repository property can become handy:
`private ClientRepository _repository => ServerInstance.ClientRepository as ClientRepository;`

#### Create a new ClientRepository
Communication to and from the clients should always run in the ClientRepository. To create your own, you will have to create a class which derives from `UnityEngineNetwork.Server.BaseClientRepository`.

Example:

    public class ClientRepository : UnityEngineNetwork.Server.BaseClientRepository

#### Start the server
To start the server, call the server start method inside your ServerNetworkManager.

    UnityEngineNetwork.Server.BaseServerNetworkManager.ServerInstance.Start(...)

#### Send packets
To send packets you have to create a method in the ClientRepository. In this method you have to call the method 
`UnityEngineNetwork.Server.BaseClientRepository.SendTCPDataToClient(int clientId, Packet packet)` ,
`UnityEngineNetwork.Server.BaseClientRepository.SendUDPDataToClient(int clientId, Packet packet)` or any other Sending method from he base class. Use the one that fits your use case the best.

Example:

    public void SendNumbers() {
	    using (Packet packet = new Packet(1)) {
	      packet.Write(3);
	      packet.Write(5);

	      SendTCPDataToClient(0, packet);
	    }
    }
##### Note: the packet id must be greater than 0. It also has to match with the packet id of the handler method of the client.

#### Handle packets
To handle any incoming packets, you have to create a new handler method in your ClientRepository. It has to match the delegate `UnityEngineNetwork.Server.Server.PacketHandler`

After you created the handler method, you have to add it to the packethandler. To do this, call the
`ServerInstance.AddPacketHandler(int id, Server.PacketHandler handler)` 
method in your ServerNetworkManager inside of 

    UnityEngineNetwork.Server.BaseServerNetworkManager.InitPacketHandlers()

Example:
##### HandlerMethod:
    public void HandleNumbers(int clientId, Packet packet) {
      int num1 = packet.ReadInt();
      int num2 = packet.ReadInt();

      Console.WriteLine($"The client {clientId} says that the sum is: {num1 + num2}!");
    }
##### Adding packet handler:
    protected override void InitPacketHandlers() {
      ServerInstance.AddPacketHandler(1, HandleNumbers);
    }
##### Note: the packet id must be greater than 0. It also has to match with the packet id of the send method of the client.
