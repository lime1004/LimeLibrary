using System.Text;

namespace LimeLibrary.UI.Module.Input {

public static class InputInteractionBuilder {
  public static string GetInteractions(InputInteractionType inputInteractionType, int? behaviourType = null) {
    if (inputInteractionType == InputInteractionType.None) return string.Empty;
    string interactions = inputInteractionType.ToString();
    if (behaviourType.HasValue) {
      var stringBuilder = new StringBuilder(interactions, 32);
      stringBuilder.Append($"(behavior={behaviourType.Value})");
      interactions = stringBuilder.ToString();
    }
    return interactions;
  }

  public static string GetRepeatInteractions(float? delaySeconds = null, float? intervalSeconds = null, float? pressPoint = null) {
    var stringBuilder = new StringBuilder("Repeat", 128);
    if (delaySeconds.HasValue) AppendParameter(stringBuilder, "_repeatDelay", $"{delaySeconds.Value:F}");
    if (intervalSeconds.HasValue) AppendParameter(stringBuilder, "_repeatInterval", $"{intervalSeconds.Value:F}");
    if (pressPoint.HasValue) AppendParameter(stringBuilder, "_pressPoint", $"{pressPoint.Value:F}");
    EndParameter(stringBuilder);

    return stringBuilder.ToString();
  }

  private static void AppendParameter(StringBuilder baseStringBuilder, string parameterName, string parameter) {
    if (baseStringBuilder[^1] == ',') {
      baseStringBuilder.Append($"{parameterName}={parameter},");
    } else {
      baseStringBuilder.Append($"({parameterName}={parameter},");
    }
  }

  private static void EndParameter(StringBuilder baseStringBuilder) {
    // 末尾の','を')'に変換
    baseStringBuilder.Remove(baseStringBuilder.Length - 1, 1);
    baseStringBuilder.Append(')');
  }
}

}