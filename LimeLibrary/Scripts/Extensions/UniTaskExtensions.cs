#if LIME_UNITASK
using System;
using Cysharp.Threading.Tasks;
using LimeLibrary.Utility;

namespace LimeLibrary.Extensions {

/// <summary>
/// UniTaskの拡張メソッド
/// </summary>
public static class UniTaskExtensions {
  /// <summary>
  /// OperationCanceledException以外の例外が発生した場合にAssertion.Assertを呼び出す
  /// </summary>
  public static async UniTask RunHandlingError(this UniTask task) {
    try {
      await task;
    } catch (Exception e) when (e is not OperationCanceledException) {
      Assertion.Assert(false, e);
    }
  }

  public static async UniTaskVoid RunHandlingErrorForget(this UniTask task) {
    try {
      await task;
    } catch (Exception e) when (e is not OperationCanceledException) {
      Assertion.Assert(false, e);
    }
  }

  public static async UniTask<T> RunHandlingError<T>(this UniTask<T> task) {
    try {
      return await task;
    } catch (Exception e) when (e is not OperationCanceledException) {
      Assertion.Assert(false, e);
    }
    return default;
  }

  public static async UniTaskVoid RunHandlingErrorForget<T>(this UniTask<T> task) {
    try {
      await task;
    } catch (Exception e) when (e is not OperationCanceledException) {
      Assertion.Assert(false, e);
    }
  }
}

}
#endif