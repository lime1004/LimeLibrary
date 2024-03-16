using System;
using UnityEngine;

namespace LimeLibrary.License {

[Serializable]
public class LicenseData {
  [SerializeField]
  private string _targetName;
  public string TargetName => _targetName;

  [SerializeField]
  private string _copyright;
  public string Copyright => _copyright;

  [SerializeField]
  private string _year;
  public string Year => _year;

  [SerializeField]
  private LicenseType _licenseType;
  public LicenseType LicenseType => _licenseType;
}

}