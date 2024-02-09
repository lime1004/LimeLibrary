using System.Collections.Generic;
using System.Text;

namespace LimeLibrary.Licence {

public class LicenceStringBuilder {
  private readonly IEnumerable<LicenceData> _licenceDataList;

  public LicenceStringBuilder(IEnumerable<LicenceData> licenceDataList) {
    _licenceDataList = licenceDataList;
  }

  public string Build() {
    var builder = new StringBuilder();
    foreach (var licenceData in _licenceDataList) {
      builder.AppendLine("-----------------------------------------------------------------------------------------------------");
      string licenceDataString = BuildLicenceData(licenceData);
      builder.AppendLine(licenceDataString);
    }
    return builder.ToString();
  }

  private string BuildLicenceData(LicenceData licenceData) {
    var builder = new StringBuilder();
    if (!string.IsNullOrEmpty(licenceData.TargetName)) {
      builder.AppendLine(licenceData.TargetName);
    }
    if (!string.IsNullOrEmpty(licenceData.Copyright)) {
      builder.Append("Copyright (c) ");
      builder.Append($"{licenceData.Year} ");
      builder.Append($"{licenceData.Copyright}");
      builder.Append("\n");
    }
    builder.Append("\n");
    builder.AppendLine(GetLicenceString(licenceData.LicenceType));

    return builder.ToString();
  }

  private string GetLicenceString(LicenceType licenceType) {
    return licenceType switch {
      LicenceType.Mit => "Licensed under MIT License (https://opensource.org/licenses/MIT)",
      LicenceType.Ofl => "Licensed under SIL Open Font License 1.1 (http://scripts.sil.org/OFL)",
      _ => string.Empty
    };
  }
}

}