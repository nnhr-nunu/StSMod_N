using System.Text;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>カード説明の1文字ずつ色付け（StS2 BBCode）。句読点は無色のまま。</summary>
public static class RainbowText
{
    /// <summary>
    /// 赤→橙→金→緑→水→青→紫の順で循環。
    /// StS2 未対応の yellow / pink は使わない（タグ名がそのまま表示される）。
    /// </summary>
    private static readonly string[] Colors =
        ["red", "orange", "gold", "green", "aqua", "blue", "purple"];

    public static string LetterByLetter(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var sb = new StringBuilder(text.Length * 16);
        var colorIndex = 0;
        foreach (var c in text)
        {
            if (IsPlainPunctuation(c))
            {
                sb.Append(c);
                continue;
            }

            var color = Colors[colorIndex % Colors.Length];
            colorIndex++;
            sb.Append('[').Append(color).Append(']').Append(c).Append("[/").Append(color).Append(']');
        }

        return sb.ToString();
    }

    private static bool IsPlainPunctuation(char c) =>
        c is '。' or '、' or '．' or '，' or '.' or ',' or '!' or '?' or '！' or '？' or ' ' or '\n';
}
