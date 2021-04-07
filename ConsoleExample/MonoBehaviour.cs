using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleExample {
  
  /// <summary>This class imitates unitys MonoBehaviour</summary>
  public abstract class MonoBehaviour {
    public int FPS { get; protected set; } = 60;

    public MonoBehaviour() {
      UpdateRecursive();
    }

    private async void UpdateRecursive() {
      await Task.Delay(1000 / FPS);
      Update();
      UpdateRecursive();
    }

    protected abstract void Update();
  }
}
