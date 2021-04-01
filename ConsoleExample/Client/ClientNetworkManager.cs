using NetworkClient = UnityEngineNetwork.Client;

namespace ConsoleExample.Client {
  public class ClientNetworkManager {

    private NetworkClient.IClient _instance => NetworkClient.Client.Instance;
    private ServerRepository _repository => (ServerRepository)_instance.ServerRepository;

    public ClientNetworkManager(ServerRepository repository) {
      _instance.ConnectToServer(repository, "TestClient", "127.0.0.1", 9999);
      _instance.AddPacketHandler((int)RequestId.SumNum, repository.HandleNumbers);
    }

    public void SendSomeNums() {
      _repository.SendNumbers();
    }

    public void Update() {
      _instance.UpdateMain();
    }
  }
}
