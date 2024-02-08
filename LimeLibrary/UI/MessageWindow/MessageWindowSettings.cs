using System;
using UnityEngine;

namespace LimeLibrary.UI.MessageWindow {

[Serializable, CreateAssetMenu(
   fileName = "MessageWindowSettings",
   menuName = "LimeLibrary/UI/MessageWindowSettings")]
public class MessageWindowSettings : ScriptableObject {
  [SerializeField]
  private float _showTextDurationEveryChar;
  public float ShowTextDurationEveryChar => _showTextDurationEveryChar;
}

}