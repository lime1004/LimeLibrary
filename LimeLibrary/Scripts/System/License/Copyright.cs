using System;
using System.Text;
using UnityEngine;

namespace LimeLibrary.License {

[Serializable]
public class Copyright {
  [SerializeField]
  private string _year;
  public string Year => _year;

  [SerializeField]
  private string _name;
  public string Name => _name;

  public void Append(StringBuilder builder) {
    builder.Append("Copyright (c) ");
    builder.Append($"{_year} ");
    builder.Append($"{_name}");
    builder.Append("\n");
  }

  public Copyright(string year, string name) => (_year, _name) = (year, name);
}

}