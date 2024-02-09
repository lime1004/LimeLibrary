using LimeLibrary.Text;

namespace LimeLibrary.UI {

public interface IUITextGetter {
  public string GetUIText(string label);
  public string GetText<TDataTable, TData>(TextQuery<TDataTable, TData> textQuery, string label) where TDataTable : ITable<TData> where TData : struct, ITextData;
}

}