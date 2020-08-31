using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngineNetwork {
  public delegate void OnWelcomeReceivedEventHandler(object sender, WelcomeEventArgs e);

  public delegate void OnClientConnectedEventHandler(object sender, ClientEventArgs e);

  public delegate void OnClientDisconnectedEventHandler(object sender, ClientEventArgs e);

  public delegate void OnDisconnectEventHandler(object sender, EventArgs e);

  public delegate void OnConnectEventHandler(object sender, EventArgs e);

  public delegate void OnServerStartedEventHandler(object sender, EventArgs e);

  public delegate void OnServerStoppedEventHandler(object sender, EventArgs e);
}
