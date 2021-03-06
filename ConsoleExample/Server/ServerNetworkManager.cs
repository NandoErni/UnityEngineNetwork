﻿using System;
using UnityEngineNetwork;
using NetworkServer = UnityEngineNetwork.Server;

namespace ConsoleExample.Server {
  public class ServerNetworkManager {

    private NetworkServer.IServer _instance => NetworkServer.Server.Instance;
    private ClientRepository _repository => (ClientRepository)_instance.ClientRepository;

    public ServerNetworkManager(ClientRepository repository) {
      _instance.Start(repository, 4, 9999);
      _instance.AddPacketHandler((int)RequestId.SumNum, repository.HandleNumbers);
    }

    public void SendSomeNums(Protocol protocol) {
      _repository.SendNumbers(protocol);
    }

    internal void SendSomeNums(object protocl) {
      throw new NotImplementedException();
    }

    public void Update() {
      _instance.UpdateMain();
    }
  }
}
