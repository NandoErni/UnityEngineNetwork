using UnityEngineNetwork;
using NetworkClient = UnityEngineNetwork.Client;

namespace ConsoleExample.Client {
  public class ClientNetworkManager {

    public NetworkClient.IClient Client { get; private set; }
    private ServerRepository _repository;

    public ClientNetworkManager() {
      Client = new NetworkClient.Client("TestClient", "127.0.0.1", 9999);
      _repository = new ServerRepository(Client);
      Client.AddPacketHandler((int)RequestId.SumNum, _repository.HandleNumbers);
      Client.ConnectToServer();
    }

    public void SendSomeNums(Protocol protocol) {
      _repository.SendNumbers(protocol);
    }

    public void Update() {
      Client.UpdateMain();
    }
  }
}
