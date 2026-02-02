using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LimeLibrary.License {

[Serializable]
public class LicenseData {
  [SerializeField]
  private string _targetName;
  public string TargetName => _targetName;

  [SerializeField]
#if LIME_ODIN_INSPECTOR
  [HideLabel]
#endif
  private Copyright _copyright;
  public Copyright Copyright => _copyright;

  [SerializeField]
  private List<Copyright> _additionalCopyrights;
  public IReadOnlyList<Copyright> AdditionalCopyrights => _additionalCopyrights;

  [SerializeField]
  private LicenseType _licenseType;
  public LicenseType LicenseType => _licenseType;
}

}