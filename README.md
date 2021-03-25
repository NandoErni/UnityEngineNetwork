
# UnityEngineNetwork
UnityEngineNetwork is a C# Library for Unity to implement networking functionalities into a game. This library is split into two main namespaces: the `UnityEngineNetwork.Server` namespace and the `UnityEngineNetwork.Client` namespace.

This library is heavily inspired by https://github.com/tom-weiland/tcp-udp-networking

## Get started
### Create a client
#### Namespaces:
 - `UnityEngineNetwork` 
 - `UnityEngineNetwork.Client`

#### Steps:
 - Create a new ServerRepository
 - Connect to the server
 - Send packets
 - Handle packets
 
#### Create a new ServerRepository
Communication to and from the server should always run in the ServerRepository. To create your own, you will have to create a class which derives from `UnityEngineNetwork.Client.BaseServerRepository`.

Example:

    public class ServerRepository : UnityEngineNetwork.Client.BaseServerRepository
    
#### Connect to the server
To connect to the server just call `UnityEngineNetwork.Client.Client.Instance.ConnectToServer(...)`.

Make sure to add your Packethandlers at this point
`UnityEngineNetwork.Client.Client.Instance.AddPacketHandler(...)`

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
To handle any incoming packets, you have to create a new handler method in your ServerRepository. It has to match the delegate `UnityEngineNetwork.Client.Client.PacketHandlerDelegate`

After you created the handler method, you have to add it to the packethandler.
`UnityEngineNetwork.Client.Client.Instance.AddPacketHandler(...)`

Example:
##### HandlerMethod:
    public void HandleNumbers(Packet packet) {
      int num1 = packet.ReadInt();
      int num2 = packet.ReadInt();

      Console.WriteLine($"The sum is: {num1 + num2}!");
    }
##### Adding packet handler:
    Client.Instance.AddPacketHandler(1, HandleNumbers);
##### Note: the packet id must be greater than 0. It also has to match with the packet id of the send method of the server.

### Create the server
#### Namespaces:
 - `UnityEngineNetwork` 
 - `UnityEngineNetwork.Server`

#### Steps:
 - Create a new ClientRepository
 - Start the server
 - Send packets
 - Handle packets
 - 
#### Create a new ClientRepository
Communication to and from the clients should always run in the ClientRepository. To create your own, you will have to create a class which derives from `UnityEngineNetwork.Server.BaseClientRepository`.

Example:

    public class ClientRepository : UnityEngineNetwork.Server.BaseClientRepository

#### Start the server
To start the server, call the server start method inside your ServerNetworkManager.

    UnityEngineNetwork.Server.BaseServerNetworkManager.ServerInstance.Start(...)

Make sure to add your Packethandlers at this point
`UnityEngineNetwork.Server.Server.Instance.AddPacketHandler(...)`

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
To handle any incoming packets, you have to create a new handler method in your ClientRepository. It has to match the delegate `UnityEngineNetwork.Server.Server.PacketHandlerDelegate`

After you created the handler method, you have to add it to the packethandler.

Example:
##### HandlerMethod:
    public void HandleNumbers(int clientId, Packet packet) {
      int num1 = packet.ReadInt();
      int num2 = packet.ReadInt();

      Console.WriteLine($"The client {clientId} says that the sum is: {num1 + num2}!");
    }
##### Adding packet handler:
    ServerInstance.AddPacketHandler(1, HandleNumbers);
##### Note: the packet id must be greater than 0. It also has to match with the packet id of the send method of the client.

