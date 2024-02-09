namespace LimeLibrary.Text {

public interface ITextData {
  public string Label { get; }
  public string SpeakerLabel { get; }
  public string GetText(Language language);
}

}