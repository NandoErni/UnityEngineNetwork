namespace UnityEngineNetwork {
  /// <summary>Contains various constants</summary>
  public sealed class Constants {
    /// <summary>The DataBufferSize.</summary>
    public const int DataBufferSize = 4096;

    /// <summary>The default port. You probably want to use your own.</summary>
    public const int DefaultPort = 25898;

    /// <summary>The localhost ip address</summary>
    public const string LocalHost = "127.0.0.1";
  }

  /// <summary>Includes protocols</summary>
  public enum Protocol {
    /// <summary>Transmission Control Protocol</summary>
    TCP,
    /// <summary>User Datagram Protocol</summary>
    UDP
  }
}
