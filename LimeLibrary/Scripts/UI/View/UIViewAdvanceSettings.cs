using System;
using UnityEngine;

namespace LimeLibrary.UI.View {

[Serializable]
public class UIViewAdvanceSettings {
  [SerializeField]
  private int _initializePriority;
  internal int InitializePriority => _initializePriority;
  [SerializeField]
  private int _focusPriority;
  internal int FocusPriority => _focusPriority;
  [SerializeField]
  private int _id;
  public int Id => _id;
}

}