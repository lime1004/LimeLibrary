namespace LimeLibrary.Text {

public static class TextIconBuilder {
  public static string BuildIconText(string name) => $"<sprite name=\"{name}\">";
  public static string BuildIconText(int index) => $"<sprite={index}>";
}

}