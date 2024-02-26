#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;

namespace LimeLibrary.Initializer {

/// <summary>
/// 外部アセットの有無に応じてDefine Symbolsを追加・削除するクラス
/// </summary>
[InitializeOnLoad]
public class DefineSymbolsManager {
  static DefineSymbolsManager() {
    AddDefineIfNecessary("UnityEngine.AddressableAssets.Addressables", "Unity.Addressables", "LIME_ADDRESSABLES");
    AddDefineIfNecessary("UnityEngine.InputSystem.InputControl", "Unity.InputSystem", "LIME_INPUTSYSTEM");
    AddDefineIfNecessary("UniRx.Observable", "UniRx", "LIME_UNIRX");
    AddDefineIfNecessary("Cysharp.Threading.Tasks.UniTask", "UniTask", "LIME_UNITASK");
    AddDefineIfNecessary("DG.Tweening.Tween", "DOTween", "LIME_DOTWEEN");
    AddDefineIfNecessary("Steamworks.SteamUtils", "com.rlabrecque.steamworks.net", "LIME_STEAMWORKS");
    AddDefineIfNecessary("Sirenix.OdinInspector.LabelTextAttribute", "Sirenix.OdinInspector.Attributes", "LIME_ODIN_INSPECTOR");
  }

  private static void AddDefineIfNecessary(string typeName, string assemblyName, string define) {
    // 現在のプラットフォームのビルド設定を取得
    var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
    string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
    // 現在のDefine Symbolsを配列に変換
    string[] definedArray = currentDefines.Split(';');

    // 指定したタイプが存在するか
    if (Type.GetType($"{typeName}, {assemblyName}") != null) {
      // シンボルがまだ定義されていない場合はシンボルを追加
      if (Array.IndexOf(definedArray, define) == -1) {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentDefines + $";{define}");
      }
    } else {
      // タイプが存在しない場合はシンボルを削除
      if (Array.IndexOf(definedArray, define) != -1) {
        string newDefines = string.Join(";", definedArray.Where(x => x != define).ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
      }
    }
  }
}

}
#endif