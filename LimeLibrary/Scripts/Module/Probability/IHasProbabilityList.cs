#if LIME_ODIN_INSPECTOR
namespace LimeLibrary.Module {

public interface IHasProbabilityList<T> {
  public ProbabilityList<T> GetProbabilityList();
}

}
#endif