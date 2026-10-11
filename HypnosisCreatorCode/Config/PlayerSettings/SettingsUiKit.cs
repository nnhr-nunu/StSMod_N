using BaseLib.Config;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;

namespace HypnosisCreator.HypnosisCreatorCode.Config.PlayerSettings;

/// <summary>
/// プレイヤー向け設定画面の部品（色付きパネル・説明文・状態表示・トグル）。
/// 見た目はちいかわMODの設定画面に合わせている。
/// </summary>
internal static class SettingsUiKit
{
    public static readonly Color DestructiveColor = new("b65c62");

    private const int ToggleWidth = 104;
    private const int ToggleHeight = 56;

    private static Texture2D? _toggleOn;
    private static Texture2D? _toggleOff;
    private static Texture2D? _toggleOnDisabled;
    private static Texture2D? _toggleOffDisabled;

    /// <summary>settings_ui の HYPNOSISCREATOR-{key}。</summary>
    public static string Text(string key) =>
        new LocString("settings_ui", "HYPNOSISCREATOR-" + key).GetFormattedText();

    public static void AddSection(
        Control parent, string name, Color accent, Action<Control> populate, int extraBottomPadding = 0)
    {
        var margin = new MarginContainer
        {
            Name = "Hc" + name + "Section",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14 + extraBottomPadding);

        var panel = new PanelContainer
        {
            Name = "SectionPanel",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Pass,
        };
        panel.AddThemeStyleboxOverride("panel", CreateSectionStyle(accent));
        margin.AddChild(panel);

        var content = new VBoxContainer
        {
            Name = "Content",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        content.AddThemeConstantOverride("separation", 16);
        panel.AddChild(content);

        populate(content);
        parent.AddChild(margin);
    }

    public static MegaRichTextLabel CreateDescription(string text, float minimumHeight)
    {
        var label = ModConfig.CreateRawLabelControl(text, 26);
        label.FitContent = true;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        label.CustomMinimumSize = new Vector2(0f, minimumHeight);
        label.AddThemeConstantOverride("line_separation", 4);
        return label;
    }

    /// <summary>結果メッセージ用。空のときは隠す（<see cref="SetStatus"/>）。</summary>
    public static MegaRichTextLabel CreateStatusLabel(float minimumHeight)
    {
        var label = CreateDescription(string.Empty, minimumHeight);
        label.Hide();
        return label;
    }

    public static void SetStatus(MegaRichTextLabel status, string text)
    {
        status.Text = text;
        status.Visible = !string.IsNullOrWhiteSpace(text);
    }

    public static HBoxContainer CreateActionRow(string name)
    {
        var row = new HBoxContainer
        {
            Name = name,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
        };
        row.AddThemeConstantOverride("separation", 14);
        return row;
    }

    /// <summary>ちいかわMODと同じカプセル型のスイッチ。</summary>
    public static CheckButton CreateToggle(string name, string label, bool pressed)
    {
        var toggle = new CheckButton
        {
            Name = name,
            Text = label,
            ButtonPressed = pressed,
            CustomMinimumSize = new Vector2(720f, 92f),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
        };
        toggle.AddThemeFontSizeOverride("font_size", 32);
        ApplyToggleTextures(toggle);
        ApplyLanguageFontFallback(toggle);
        return toggle;
    }

    private static StyleBoxFlat CreateSectionStyle(Color accent)
    {
        var background = new Color("18212a").Lerp(accent, 0.04f);
        background.A = 0.78f;
        var border = accent;
        border.A = 0.45f;
        var style = new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            ContentMarginLeft = 22f,
            ContentMarginRight = 22f,
            ContentMarginTop = 20f,
            ContentMarginBottom = 20f,
        };
        style.SetBorderWidthAll(1);
        style.BorderWidthLeft = 3;
        style.SetCornerRadiusAll(12);
        return style;
    }

    private static void ApplyToggleTextures(CheckButton toggle)
    {
        _toggleOn ??= CreateToggleTexture(enabled: true, disabled: false);
        _toggleOff ??= CreateToggleTexture(enabled: false, disabled: false);
        _toggleOnDisabled ??= CreateToggleTexture(enabled: true, disabled: true);
        _toggleOffDisabled ??= CreateToggleTexture(enabled: false, disabled: true);

        foreach (var suffix in new[] { "", "_mirrored" })
        {
            toggle.AddThemeIconOverride("checked" + suffix, _toggleOn);
            toggle.AddThemeIconOverride("unchecked" + suffix, _toggleOff);
            toggle.AddThemeIconOverride("checked_disabled" + suffix, _toggleOnDisabled);
            toggle.AddThemeIconOverride("unchecked_disabled" + suffix, _toggleOffDisabled);
        }

        toggle.AddThemeConstantOverride("check_v_offset", 0);
    }

    private static Texture2D CreateToggleTexture(bool enabled, bool disabled)
    {
        var image = Image.CreateEmpty(ToggleWidth, ToggleHeight, false, Image.Format.Rgba8);
        var track = disabled ? new Color("4d5258") : enabled ? new Color("4fa56f") : new Color("686d74");
        var rim = disabled ? new Color("70757b") : new Color("d8ddd9");
        var knob = disabled ? new Color("a4a8ac") : Colors.White;
        const float trackRadius = 24f;
        const float knobRadius = 19f;
        var left = new Vector2(28f, 28f);
        var right = new Vector2(76f, 28f);
        var knobCenter = enabled ? right : left;

        for (var y = 0; y < ToggleHeight; y++)
        {
            for (var x = 0; x < ToggleWidth; x++)
            {
                var point = new Vector2(x + 0.5f, y + 0.5f);
                var distance = CapsuleDistance(point, left, right);
                if (distance <= trackRadius)
                    image.SetPixel(x, y, distance >= trackRadius - 2f ? rim : track);
                if (point.DistanceTo(knobCenter) <= knobRadius)
                    image.SetPixel(x, y, knob);
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static float CapsuleDistance(Vector2 point, Vector2 left, Vector2 right)
    {
        if (point.X < left.X)
            return point.DistanceTo(left);
        if (point.X > right.X)
            return point.DistanceTo(right);
        return MathF.Abs(point.Y - left.Y);
    }

    /// <summary>
    /// CheckButton の既定フォントは日本語などを持たないことがあるので、表示言語の代替フォントを先頭に足す。
    /// </summary>
    private static void ApplyLanguageFontFallback(Control control)
    {
        if (control.IsInsideTree() && control.IsNodeReady())
            AddFallback(control);
        else
            control.Ready += () => AddFallback(control);
    }

    private static void AddFallback(Control control)
    {
        try
        {
            var themeFont = control.GetThemeFont("font");
            if (themeFont == null)
                return;

            var language = LocManager.Instance?.Language;
            var substitute = language == null ? null : FontManager.GetSubstituteFont(language, FontType.Regular);
            var fallbacks = new Godot.Collections.Array<Font>();
            if (substitute != null && substitute != themeFont)
                fallbacks.Add(substitute);
            foreach (var font in themeFont.Fallbacks)
            {
                if (font != null && font != themeFont && !fallbacks.Contains(font))
                    fallbacks.Add(font);
            }

            if (ThemeDB.FallbackFont is { } global && global != themeFont && !fallbacks.Contains(global))
                fallbacks.Add(global);
            if (fallbacks.Count == 0)
                return;

            var variation = themeFont is FontVariation existing
                ? (FontVariation)existing.Duplicate()
                : new FontVariation { BaseFont = themeFont };
            variation.Fallbacks = fallbacks;
            control.AddThemeFontOverride("font", variation);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Settings toggle font fallback skipped: {ex.Message}");
        }
    }
}
