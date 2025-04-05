using Cysharp.Threading.Tasks;

namespace LimeLibrary.UI.App {

public interface IUIAppFlow {
  public UniTask Start();
  public void Stop();
}

}