using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LimeLibrary.Module;
using LimeLibrary.Text;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TextCore.LowLevel;

#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace Next.GameSystem.Text {

[Serializable, CreateAssetMenu(
   fileName = "FontAssetInitializer",
   menuName = "LimeLibrary/Font/FontAssetInitializer")]
public class FontAssetInitializer : ScriptableObject
#if UNITY_EDITOR
  , IPreprocessBuildWithReport
#endif
{
  [SerializeField]
  private AssetReferenceT<TMP_FontAsset> _baseFontAssetReference;
  [SerializeField]
  private FontAssetDataDictionary _fontAssetDataDictionary;

  [Serializable]
  private class FontAssetData {
    [SerializeField]
    private List<AssetReferenceT<TMP_FontAsset>> _fallbackFontAssetReferenceList;
    public IReadOnlyList<AssetReferenceT<TMP_FontAsset>> FallbackFontAssetReferenceList => _fallbackFontAssetReferenceList;
  }

  [Serializable]
  private class FontAssetDataDictionary : SerializedDictionary<Language, FontAssetData> { }

  public async UniTask Initialize(Language language, CancellationToken cancellationToken) {
    if (!_fontAssetDataDictionary.TryGetValue(language, out var fontAssetData)) return;

    // ベースのフォントアセットのロード
    await _baseFontAssetReference.LoadAssetAsync().ToUniTask(cancellationToken: cancellationToken);
    var baseFontAsset = (TMP_FontAsset) _baseFontAssetReference.Asset;

    // フォールバックのフォントアセットのロード
    var fallbackFontAssetTaskList = new List<UniTask>();
    foreach (var fallbackFontAssetReference in fontAssetData.FallbackFontAssetReferenceList) {
      fallbackFontAssetTaskList.Add(fallbackFontAssetReference.LoadAssetAsync().ToUniTask(cancellationToken: cancellationToken));
    }
    await fallbackFontAssetTaskList;

    // フォールバックのフォントアセットの設定  
    baseFontAsset.fallbackFontAssetTable.Clear();
    bool isEditor = Application.installMode == ApplicationInstallMode.Editor;
    foreach (var fallbackFontAssetReference in fontAssetData.FallbackFontAssetReferenceList) {
      var fallbackFontAsset = (TMP_FontAsset) fallbackFontAssetReference.Asset;
      bool isDynamic = fallbackFontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic;

      // EditorでそのままDynamicFontを使うと上書きされてくので複製
      if (isEditor && isDynamic) {
        var source = fallbackFontAsset;
        fallbackFontAsset = TMP_FontAsset.CreateFontAsset(
          source.sourceFontFile,
          source.creationSettings.pointSize,
          source.atlasPadding,
          GlyphRenderMode.SDFAA,
          source.atlasWidth,
          source.atlasHeight,
          AtlasPopulationMode.Dynamic,
          source.isMultiAtlasTexturesEnabled);
      }

      baseFontAsset.fallbackFontAssetTable.Add(fallbackFontAsset);
    }

    // 解放処理
    var fontDisposerObject = new GameObject("FontDisposer");
    DontDestroyOnLoad(fontDisposerObject);
    fontDisposerObject.OnDestroyAsObservable().Subscribe(x => {
      if (isEditor) Resources.UnloadAsset(baseFontAsset);
      if (_baseFontAssetReference.Asset) _baseFontAssetReference.ReleaseAsset();
      foreach (var fallbackFontAssetReference in fontAssetData.FallbackFontAssetReferenceList) {
        if (fallbackFontAssetReference.Asset) fallbackFontAssetReference.ReleaseAsset();
      }
    });
  }

#if UNITY_EDITOR
  public int callbackOrder => 0;
  public void OnPreprocessBuild(BuildReport report) {
    // ベースのフォントアセットのフォールバックを剥がす
    var baseFontAsset = _baseFontAssetReference.editorAsset;
    baseFontAsset.fallbackFontAssetTable.Clear();
  }
#endif
}

}