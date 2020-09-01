# UnityEngineNetwork
C# Library for Unity to implement networking functionalities into a game
## Get started
### Create a client
#### Steps:
 - Create a new ClientNetworkManager
 - Create a new ServerRepository
 - Connect to the Server
 - Send Packets
 - Handle Packets
 
#### Create a new ClientNetworkManager
This library provides a ClientNetworkManager base class, which is used to manage everything done to the client. To use the ClientNetworkManager base class, create a new class, which derives from `UnityEngineNetwork.Client.BaseClientNetworkManager`

A repository property can become handy:
`private ServerRepository _repository => ClientInstance.ServerRepository as ServerRepository;`

Example:

    public class ClientNetworkManager : UnityEngineNetwork.Client.BaseClientNetworkManager

#### Create a new ServerRepository
Communication to and from the server should always run in the ServerRepository. To create your own, you will have to create a class which derives from `UnityEngineNetwork.Client.BaseServerRepository`.

Example:

    public class ServerRepository : UnityEngineNetwork.Client.BaseServerRepository
    
#### Connect to the Server
To connect to the server just call `UnityEngineNetwork.Client.BaseClientNetworkManager.ClientInstance.ConnectToServer(...)`.
Do this in your own ClientNetworkManager.

#### Send Packets
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

#### Handle Packets
To handle any incomming packets, you have to create a new handler method in your ServerRepository. It has to math the delegate `UnityEngineNetwork.Client.Client.PacketHandler`

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
Todo
