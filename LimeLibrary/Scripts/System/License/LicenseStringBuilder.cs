using System.Collections.Generic;
using System.Text;

namespace LimeLibrary.License {

public class LicenseStringBuilder {
  private readonly IEnumerable<LicenseData> _licenceDataList;

  public LicenseStringBuilder(IEnumerable<LicenseData> licenceDataList) {
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

  private string BuildLicenceData(LicenseData licenseData) {
    var builder = new StringBuilder();
    if (!string.IsNullOrEmpty(licenseData.TargetName)) {
      builder.AppendLine(licenseData.TargetName);
    }
    licenseData.Copyright.Append(builder);
    foreach (var copyright in licenseData.AdditionalCopyrights) {
      copyright.Append(builder);
    }
    builder.Append("\n");
    builder.AppendLine(GetLicenceString(licenseData.LicenseType));

    return builder.ToString();
  }

  private string GetLicenceString(LicenseType licenseType) {
    return licenseType switch {
      LicenseType.Mit => "Licensed under MIT License (https://opensource.org/licenses/MIT)",
      LicenseType.Ofl => "Licensed under SIL Open Font License 1.1 (http://scripts.sil.org/OFL)",
      _ => string.Empty
    };
  }
}

}