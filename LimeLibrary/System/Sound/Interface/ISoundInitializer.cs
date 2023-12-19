using Cysharp.Threading.Tasks;

namespace LimeLibrary.Sound {

public interface ISoundInitializer {
  public UniTask Initialize();
}

}