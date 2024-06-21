namespace LimeLibrary.Debug.ImGui {

public interface IMenuContent {
  public void Initialize();
  public void Execute();
  public void OnDestroy();
}

}