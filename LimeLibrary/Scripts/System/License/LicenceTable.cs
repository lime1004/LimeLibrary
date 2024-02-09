using System;
using System.Collections.Generic;
using UnityEngine;

namespace LimeLibrary.Licence {

[Serializable, CreateAssetMenu(
   fileName = "LicenceTable",
   menuName = "LimeLibrary/Licence/LicenceTable")]
public class LicenceTable : ScriptableObject {
  [SerializeField]
  private List<LicenceData> _licenceDataList;
  public IReadOnlyList<LicenceData> LicenceDataList => _licenceDataList;
}

}