using System;

namespace UnityEngineNetwork {
  /// <summary>Event handler for the welcome message</summary>
  /// <param name="sender">The sender</param>
  /// <param name="e">The welcome event args</param>
  public delegate void OnWelcomeReceivedEventHandler(object sender, WelcomeEventArgs e);

  /// <summary>Event handler for the client connected event</summary>
  /// <param name="sender">The sender</param>
  /// <param name="e">The event args</param>
  public delegate void OnClientConnectedEventHandler(object sender, ClientEventArgs e);

  /// <summary>Event handler for the client diconect event.</summary>
  /// <param name="sender">The sender</param>
  /// <param name="e">The event args</param>
  public delegate void OnClientDisconnectedEventHandler(object sender, ClientEventArgs e);

  /// <summary>Event handler for the diconnected event</summary>
  /// <param name="sender">The sender</param>
  /// <param name="e">The event args</param>
  public delegate void OnDisconnectEventHandler(object sender, EventArgs e);

  /// <summary>Event handler for the connected event</summary>
  /// <param name="sender">The sender</param>
  /// <param name="e">The event args</param>
  public delegate void OnConnectEventHandler(object sender, EventArgs e);

  /// <summary>Event handler for the server started event</summary>
  /// <param name="sender">The sender</param>
  /// <param name="e">The event args</param>
  public delegate void OnServerStartedEventHandler(object sender, EventArgs e);

  /// <summary>Event handler for the server stopped event</summary>
  /// <param name="sender">The sender</param>
  /// <param name="e">The event args</param>
  public delegate void OnServerStoppedEventHandler(object sender, EventArgs e);
}
