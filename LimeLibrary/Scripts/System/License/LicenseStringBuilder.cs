using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LimeLibrary.License {

public class LicenseStringBuilder {
  // デフォルト幅はLicenseFullTextの本文の折り返し幅（最長78文字）に合わせている
  private const int DefaultCopyrightSeparatorWidth = 78;
  private const int DefaultLicenseFullTextSeparatorWidth = 78;

  private readonly IEnumerable<LicenseData> _licenceDataList;
  private readonly int _copyrightSeparatorWidth;
  private readonly int _licenseFullTextSeparatorWidth;

  public LicenseStringBuilder(
    IEnumerable<LicenseData> licenceDataList,
    int copyrightSeparatorWidth = DefaultCopyrightSeparatorWidth,
    int licenseFullTextSeparatorWidth = DefaultLicenseFullTextSeparatorWidth) {
    if (copyrightSeparatorWidth < 1) throw new ArgumentOutOfRangeException(nameof(copyrightSeparatorWidth));
    if (licenseFullTextSeparatorWidth < 1) throw new ArgumentOutOfRangeException(nameof(licenseFullTextSeparatorWidth));
    _licenceDataList = licenceDataList ?? Enumerable.Empty<LicenseData>();
    _copyrightSeparatorWidth = copyrightSeparatorWidth;
    _licenseFullTextSeparatorWidth = licenseFullTextSeparatorWidth;
  }

  public string Build() {
    var builder = new StringBuilder();
    var usedLicenseTypes = new List<LicenseType>();
    foreach (var licenceData in _licenceDataList) {
      builder.AppendLine(new string('-', _copyrightSeparatorWidth));
      string licenceDataString = BuildLicenceData(licenceData);
      builder.AppendLine(licenceDataString);

      if (!usedLicenseTypes.Contains(licenceData.LicenseType)) {
        usedLicenseTypes.Add(licenceData.LicenseType);
      }
    }

    // 各ライブラリのCopyright表示に加え、使用されているライセンス種別の全文を1種類につき1回だけ末尾に載せる。
    // (MIT/OFLはいずれも著作権表示に加えて許諾表示・ライセンス本文の同梱を要求するため)
    foreach (var licenseType in usedLicenseTypes) {
      string fullText = LicenseFullText.Get(licenseType);
      if (string.IsNullOrEmpty(fullText)) continue;

      builder.AppendLine(new string('=', _licenseFullTextSeparatorWidth));
      builder.AppendLine(GetLicenceString(licenseType));
      builder.AppendLine();
      builder.AppendLine(fullText);
      builder.AppendLine();
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