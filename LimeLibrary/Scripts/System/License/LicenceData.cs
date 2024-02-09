using System;
using UnityEngine;

namespace LimeLibrary.Licence {

[Serializable]
public class LicenceData {
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
  private LicenceType _licenceType;
  public LicenceType LicenceType => _licenceType;
}

}