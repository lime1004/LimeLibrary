namespace LimeLibrary.Text {

public interface ITextQuery {
  public ITextData GetTextData(string label);
  public string GetText(string label, Language language);
  public string GetSpeakerLabel(string label);
  public bool ExistsLabel(string label);
}

}