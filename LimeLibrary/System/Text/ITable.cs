using System.Collections.Generic;

namespace LimeLibrary.Text {

public interface ITable<out T> {
  public IReadOnlyList<T> List { get; }
}

}