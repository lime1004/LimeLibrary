using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LimeLibrary.License {

[Serializable]
public class LicenseData {
  [SerializeField]
  private string _targetName;
  public string TargetName => _targetName;

  [SerializeField, FormerlySerializedAs("_copyright")]
  private string _copyright_;
  public string Copyright_ => _copyright_;

  [SerializeField]
  private string _year;
  public string Year => _year;

  [SerializeField]
  private Copyright _copyright;
  public Copyright Copyright => _copyright;

  [SerializeField]
  private List<Copyright> _additionalCopyrights;
  public IReadOnlyList<Copyright> AdditionalCopyrights => _additionalCopyrights;

  [SerializeField]
  private LicenseType _licenseType;
  public LicenseType LicenseType => _licenseType;

  public void OnValidate() {
    _copyright = new Copyright(_year, _copyright_);
  }
}

}