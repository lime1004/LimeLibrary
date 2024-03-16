using System;
using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.License {

[Serializable, CreateAssetMenu(
   fileName = "LicenseTable",
   menuName = "LimeLibrary/License/LicenseTable")]
public class LicenseTable : ScriptableObject {
  [SerializeField]
  private List<LicenseData> _licenceDataList;
  public IReadOnlyList<LicenseData> LicenceDataList => _licenceDataList;
}

}