#if LIME_ODIN_INSPECTOR
namespace LimeLibrary.Module {

public interface IHasProbabilityList<T> {
  internal ProbabilityList<T> GetProbabilityList();
}

}
#endif