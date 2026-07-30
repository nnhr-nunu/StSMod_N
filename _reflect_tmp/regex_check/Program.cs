using System;
using System.Text.RegularExpressions;
public class T {
  static readonly Regex R = new(@"^\[gold\](?:保留|Retain)\[/gold\][。.](?:\r?\n)?", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Multiline);
  public static void Main() {
    var samples = new[] {
      "[gold]カウント[/gold]。\n[gold]保留[/gold]。\n3ターン後、相手の心臓が止まる。",
      "[gold]Retain[/gold].\n[gold]Count[/gold].\nAfter 3 turns.",
      "[gold]アブノーマル[/gold]。\n[gold]カウント[/gold]。\n[gold]保留[/gold]。\n毒を付与する。",
      "[gold]カウント[/gold]。\n[gold]保留[/gold]。"
    };
    foreach (var s in samples) {
      var o = R.Replace(s, "");
      Console.WriteLine("---");
      Console.WriteLine(o.Replace("\n", "\\n"));
      Console.WriteLine("has blank=" + o.Contains("\n\n"));
    }
  }
}
