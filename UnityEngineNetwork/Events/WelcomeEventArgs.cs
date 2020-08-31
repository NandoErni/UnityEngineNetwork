using System;

namespace UnityEngineNetwork {
  public class WelcomeEventArgs : EventArgs {
    public string Message;

    public WelcomeEventArgs(string message) {
      Message = message;
    }
  }
}
