using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.System;

namespace LimeLibrary.Text {

public static class TextLoader {
  /// <summary>
  /// Textのパスからデータを読み込みTextQueryとして返す
  /// </summary>
  public static TextQuery<TDataTable, TData> Load<TDataTable, TData>(string path)
    where TDataTable : class, ITable<TData> where TData : struct, ITextData {
    using var textDataTable = ResourceLoader.Load<TDataTable>(path);
    var textQuery = new TextQuery<TDataTable, TData>(textDataTable.Resource);
    return textQuery;
  }

#if LIME_UNITASK
  /// <summary>
  /// Textのパスからデータを読み込みTextQueryとして返す
  /// </summary>
  public static async UniTask<TextQuery<TDataTable, TData>> LoadAsync<TDataTable, TData>(string path, CancellationToken cancellationToken)
    where TDataTable : class, ITable<TData> where TData : struct, ITextData {
    using var textDataTable = await ResourceLoader.LoadAsync<TDataTable>(path, cancellationToken);
    var textQuery = new TextQuery<TDataTable, TData>(textDataTable.Resource);
    return textQuery;
  }
#endif
}

}