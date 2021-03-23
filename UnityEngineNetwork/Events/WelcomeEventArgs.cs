using System;

namespace UnityEngineNetwork {
  /// <summary>EventArgs for the first message.</summary>
  public class WelcomeEventArgs : EventArgs {
    /// <summary>The welcome message</summary>
    public string Message;

    /// <summary>Create EventArgs for the first message.</summary>
    /// <param name="message">The welcome message.</param>
    public WelcomeEventArgs(string message) {
      Message = message;
    }
  }
}
