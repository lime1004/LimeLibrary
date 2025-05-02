using System.Collections.Generic;
using System.Linq;
using System.Text;
using LimeLibrary.Attributes;
using LimeLibrary.Extensions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LimeLibrary.Module {

public class Locator : MonoBehaviour {
  private readonly Dictionary<string, Transform> _locatorDictionary = new(32);

  private void OnEnable() {
    Setup();
  }

  public void Setup() {
    var rootTransform = transform;
    _locatorDictionary.Clear();
    _locatorDictionary.Add(rootTransform.name, rootTransform);
    RegisterChildTransform(rootTransform, rootTransform.name);
  }

  private void RegisterChildTransform(Transform rootTransform, string parentPath) {
    foreach (var child in rootTransform.GetChildList()) {
      string path = parentPath + "/" + child.name;
      _locatorDictionary.Add(path, child);
      RegisterChildTransform(child, path);
    }
  }

  /// <summary>
  /// LocatorのTransformを取得
  /// </summary>
  /// <param name="locatorName">Locator名</param>
  public Transform GetLocatorTransform(string locatorName) {
    bool isSimpleName = !locatorName.Contains("/");
    foreach ((string locatorPath, var locatorTransform) in _locatorDictionary) {
      if (isSimpleName) {
        if (locatorPath.Split('/').LastOrDefault() == locatorName) return locatorTransform;
      } else {
        if (IsMatchLocatorName(locatorName, locatorPath)) return locatorTransform;
      }
    }
    return null;
  }

  /// <summary>
  /// Locatorが存在するか
  /// </summary>
  /// <param name="locatorName">Locator名</param>
  public bool ExistsLocator(string locatorName) {
    bool isSimpleName = !locatorName.Contains("/");
    foreach ((string locatorPath, _) in _locatorDictionary) {
      if (isSimpleName) {
        if (locatorPath.Split('/').LastOrDefault() == locatorName) return true;
      } else {
        if (IsMatchLocatorName(locatorPath, locatorPath)) return true;
      }
    }
    return false;
  }

  /// <summary>
  /// Locator名がLocatorパスの中から一致するか
  /// </summary>
  private static bool IsMatchLocatorName(string locatorName, string locatorPath) {
    string[] separatedBonePaths = locatorPath.Split('/');
    var buildPath = new StringBuilder("");
    for (int i = separatedBonePaths.Length - 1; i >= 0; i--) {
      buildPath.Insert(0, separatedBonePaths[i]);
      if (buildPath.ToString() == locatorName) return true;
      buildPath.Insert(0, "/");
    }
    return false;
  }

#if UNITY_EDITOR
  [Button("CopyLocator")]
  private bool _copyLocator;

  public void CopyLocator() {
    string copyString = string.Empty;
    Setup();
    foreach ((string locatorName, var locatorTransform) in _locatorDictionary) {
      copyString += locatorName + "_";
      var position = locatorTransform.localPosition;
      copyString += position.ToString() + "_";
      var rotation = locatorTransform.localRotation;
      copyString += rotation.eulerAngles.ToString() + "_";
      var scale = locatorTransform.localScale;
      copyString += scale.ToString();
      copyString += "+";
    }
    EditorGUIUtility.systemCopyBuffer = copyString;
  }

  [Button("PasteLocator")]
  private bool _pasteLocator;

  public void PasteLocator() {
    string copyString = EditorGUIUtility.systemCopyBuffer;
    if (string.IsNullOrEmpty(copyString)) return;
    Setup();

    string[] groupSplitString = copyString.Split("+");
    foreach (string groupString in groupSplitString) {
      if (string.IsNullOrEmpty(groupString)) return;
      string[] splitString = groupString.Split("_");
      string locatorName = splitString[0];
      var position = ToVector3(splitString[1]);
      var rotation = ToVector3(splitString[2]);
      var scale = ToVector3(splitString[3]);

      if (_locatorDictionary.TryGetValue(locatorName, out var value)) {
        var locatorTransform = value.transform;
        locatorTransform.localPosition = position;
        locatorTransform.localRotation = Quaternion.Euler(rotation);
        locatorTransform.localScale = scale;
      }
    }
  }

  private static Vector3 ToVector3(string stringVector3) {
    stringVector3 = stringVector3.Trim('(', ')');
    string[] splitString = stringVector3.Split(",");
    var result = Vector3.zero;
    for (int i = 0; i < 3; i++) {
      string floatString = splitString[i];
      if (float.TryParse(floatString.Trim(), out float parse)) {
        result[i] = parse;
      }
    }
    return result;
  }
#endif
}

}