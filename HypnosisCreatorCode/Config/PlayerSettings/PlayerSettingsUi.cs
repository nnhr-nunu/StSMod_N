using BaseLib.Config.UI;
using Godot;
using HypnosisCreator.HypnosisCreatorCode.Telemetry;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using static HypnosisCreator.HypnosisCreatorCode.Config.PlayerSettings.SettingsUiKit;

namespace HypnosisCreator.HypnosisCreatorCode.Config.PlayerSettings;

/// <summary>
/// Mod設定（Hypno Creator）のプレイヤー向け画面。
/// フィードバック・アセンション解放（シングル／マルチ）・匿名データ送信・全要素の解放。
/// </summary>
internal sealed class PlayerSettingsUi
{
    private static readonly Color FeedbackColor = new("7aa7d4");
    private static readonly Color AscensionColor = new("71b0b5");
    private static readonly Color MultiplayerAscensionColor = new("a18bc9");
    private static readonly Color DataUploadColor = new("72b68c");
    private static readonly Color FullUnlockColor = new("d2a05c");

    private readonly HypnosisCreatorConfig _config;
    private readonly List<AscensionChecks> _ascensionChecks = [];

    private PlayerSettingsUi(HypnosisCreatorConfig config) => _config = config;

    public static void Build(HypnosisCreatorConfig config, Control optionContainer)
    {
        var ui = new PlayerSettingsUi(config);
        AddSection(optionContainer, "Feedback", FeedbackColor, ui.AddFeedback, extraBottomPadding: 20);
        AddSection(optionContainer, "SingleplayerAscension", AscensionColor, c => ui.AddAscension(c, multiplayer: false));
        AddSection(optionContainer, "MultiplayerAscension", MultiplayerAscensionColor, c => ui.AddAscension(c, multiplayer: true));
        AddSection(optionContainer, "DataUpload", DataUploadColor, ui.AddDataUpload);
        AddSection(optionContainer, "FullUnlock", FullUnlockColor, ui.AddFullUnlock);
    }

    // --- フィードバック ---

    private void AddFeedback(Control section)
    {
        var editor = new VBoxContainer
        {
            Name = "FeedbackEditor",
            Visible = false,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        editor.AddThemeConstantOverride("separation", 10);

        var input = new NMegaTextEdit
        {
            Name = "FeedbackInput",
            PlaceholderText = Text("FEEDBACK.placeholder"),
            WrapMode = TextEdit.LineWrappingMode.Boundary,
            CustomMinimumSize = new Vector2(720f, 260f),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        input.AddThemeFontSizeOverride("font_size", 25);
        var counter = BaseLib.Config.ModConfig.CreateRawLabelControl(Counter(0), 20);
        var status = CreateStatusLabel(56f);

        // 上限を超える入力（貼り付け含む）は直前の内容に戻す
        var acceptedText = string.Empty;
        input.TextChanged += () =>
        {
            if (input.Text.Length > FeedbackSender.MaxCharacters)
            {
                input.Text = acceptedText;
                var lastLine = input.GetLineCount() - 1;
                input.SetCaretLine(lastLine);
                input.SetCaretColumn(input.GetLine(lastLine).Length);
            }
            else
            {
                acceptedText = input.Text;
            }

            counter.Text = Counter(input.Text.Length);
        };

        var sending = false;
        var actions = CreateActionRow("FeedbackActions");
        var send = _config.CreateButton(Text("FEEDBACK.send"), Submit);
        send.CustomMinimumSize = new Vector2(260f, 72f);
        var close = _config.CreateButton(Text("FEEDBACK.close"), () => editor.Visible = false);
        close.CustomMinimumSize = new Vector2(220f, 72f);
        actions.AddChild(send);
        actions.AddChild(close);

        editor.AddChild(CreateDescription(Text("FEEDBACK.description"), 40f));
        editor.AddChild(input);
        editor.AddChild(counter);
        editor.AddChild(actions);
        editor.AddChild(status);

        var open = _config.CreateButton(Text("FEEDBACK.open"), () =>
        {
            editor.Visible = !editor.Visible;
            if (editor.Visible)
                input.GrabFocus();
        });
        open.Name = "OpenFeedback";
        open.SetColor(new Color("477fb8"));
        open.CustomMinimumSize = new Vector2(420f, 76f);
        open.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

        section.AddChild(open);
        section.AddChild(editor);
        return;

        async void Submit()
        {
            if (sending)
                return;
            if (string.IsNullOrWhiteSpace(input.Text))
            {
                SetStatus(status, Text("FEEDBACK.required"));
                return;
            }

            sending = true;
            input.Editable = false;
            SetStatus(status, Text("FEEDBACK.sending"));
            FeedbackSendResult result;
            try
            {
                result = await FeedbackSender.SendAsync(input.Text);
            }
            catch (Exception ex)
            {
                MainFile.Logger.Warn($"Feedback send failed: {ex.Message}");
                result = FeedbackSendResult.Failed;
            }

            // 送信中に設定画面を閉じていたら何もしない
            if (!GodotObject.IsInstanceValid(status) || !GodotObject.IsInstanceValid(input))
                return;

            sending = false;
            input.Editable = true;
            SetStatus(status, Text("FEEDBACK." + result switch
            {
                FeedbackSendResult.Sent => "sent",
                FeedbackSendResult.Busy => "busy",
                FeedbackSendResult.RateLimited => "rate_limited",
                FeedbackSendResult.Invalid => "invalid",
                _ => "failed",
            }));
            if (result == FeedbackSendResult.Sent)
            {
                input.Text = string.Empty;
                acceptedText = string.Empty;
            }
        }
    }

    private static string Counter(int length) => $"{length} / {FeedbackSender.MaxCharacters}";

    // --- アセンション解放 ---

    private void AddAscension(Control section, bool multiplayer)
    {
        var prefix = multiplayer ? "UNLOCK_MULTIPLAYER_ASCENSION." : "UNLOCK_ASCENSION.";
        var scope = multiplayer ? "Multiplayer" : "Singleplayer";
        section.AddChild(CreateDescription(Text(prefix + "description"), 40f));
        var status = CreateStatusLabel(88f);

        var grid = new GridContainer
        {
            Name = scope + "AscensionButtons",
            Columns = 5,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
        };
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 10);

        var checks = new AscensionChecks(multiplayer);
        _ascensionChecks.Add(checks);
        for (var level = 1; level <= AscensionUnlock.Maximum; level++)
        {
            var target = level;
            var button = _config.CreateButton($"A{target}",
                () => ShowAscensionResult(status, AscensionUnlock.UnlockThrough(target, multiplayer), prefix + "failed"));
            button.Name = $"Unlock{scope}Ascension{target}";
            button.CustomMinimumSize = new Vector2(158f, 72f);
            button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            if (multiplayer)
                button.SetColor(new Color("7968a6"));
            grid.AddChild(button);
            checks.Register(button, target);
        }

        var reset = _config.CreateButton(Text(prefix + "reset_button"),
            () => ShowAscensionResult(status, AscensionUnlock.Reset(multiplayer), prefix + "reset_failed"));
        reset.Name = $"Reset{scope}Ascension";
        reset.CustomMinimumSize = new Vector2(360f, 72f);
        reset.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        reset.SetColor(DestructiveColor);
        var actions = CreateActionRow(scope + "AscensionActions");
        actions.AddChild(reset);

        section.AddChild(grid);
        section.AddChild(actions);
        section.AddChild(status);
        checks.Refresh();
        return;

        // 成功時はチェック表示で分かるので文言は出さない（ちいかわMODと同じ）
        void ShowAscensionResult(MegaRichTextLabel statusLabel, ProgressEditResult result, string failedKey)
        {
            SetStatus(statusLabel, result switch
            {
                ProgressEditResult.Changed or ProgressEditResult.AlreadyDone => string.Empty,
                ProgressEditResult.RunInProgress => Text(prefix + "run_in_progress"),
                _ => Text(failedKey),
            });
            RefreshAscensionChecks();
        }
    }

    private void RefreshAscensionChecks()
    {
        foreach (var checks in _ascensionChecks)
            checks.Refresh();
    }

    /// <summary>A1〜A10 ボタン右端の「解放済み」チェック。</summary>
    private sealed class AscensionChecks(bool multiplayer)
    {
        private readonly List<(CheckBox Indicator, int Level)> _indicators = [];

        public void Register(NConfigButton button, int level)
        {
            var indicator = new CheckBox
            {
                Name = "AscensionUnlockedIndicator",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                FocusMode = Control.FocusModeEnum.None,
                CustomMinimumSize = new Vector2(32f, 32f),
            };
            indicator.SetAnchorsPreset(Control.LayoutPreset.CenterRight);
            indicator.OffsetLeft = -40f;
            indicator.OffsetRight = -8f;
            indicator.OffsetTop = -16f;
            indicator.OffsetBottom = 16f;
            button.AddChild(indicator);

            // ボタン文字がチェックに重ならないよう寄せる
            if (button.GetNodeOrNull<Label>("Label") is { } label)
            {
                label.OffsetLeft = 8f;
                label.OffsetRight = -40f;
            }

            _indicators.Add((indicator, level));
        }

        public void Refresh()
        {
            var maximum = AscensionUnlock.ReadMaximum(multiplayer);
            foreach (var (indicator, level) in _indicators)
            {
                if (GodotObject.IsInstanceValid(indicator))
                    indicator.SetPressedNoSignal(maximum.HasValue && level <= maximum.Value);
            }
        }
    }

    // --- 匿名データ送信 ---

    private void AddDataUpload(Control section)
    {
        section.AddChild(CreateDescription(Text("DATA_UPLOAD.description"), 72f));
        var available = UploadDataSetting.TryGet(out var enabled);
        var status = CreateStatusLabel(0f);
        SetStatus(status, available ? string.Empty : Text("DATA_UPLOAD.unavailable"));

        var toggle = CreateToggle("DataUploadToggle", Text("DATA_UPLOAD.label"), enabled);
        toggle.Disabled = !available;
        toggle.Toggled += requested =>
        {
            if (UploadDataSetting.TrySet(requested))
            {
                SetStatus(status, string.Empty);
                return;
            }

            toggle.SetPressedNoSignal(!requested);
            SetStatus(status, Text("DATA_UPLOAD.failed"));
        };

        section.AddChild(toggle);
        section.AddChild(status);
    }

    // --- 全要素の解放 ---

    private void AddFullUnlock(Control section)
    {
        section.AddChild(CreateDescription(Text("FULL_UNLOCK.description"), 40f));
        var status = CreateStatusLabel(64f);
        ProgressState? confirmedProgress = null;

        var confirmation = new PanelContainer
        {
            Name = "FullUnlockConfirmation",
            Visible = false,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        var style = new StyleBoxFlat
        {
            BgColor = new Color("30251b"),
            BorderColor = new Color("d3a14c"),
            ContentMarginLeft = 22f,
            ContentMarginRight = 22f,
            ContentMarginTop = 18f,
            ContentMarginBottom = 18f,
        };
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(10);
        confirmation.AddThemeStyleboxOverride("panel", style);

        var box = new VBoxContainer();
        box.AddThemeConstantOverride("separation", 16);
        confirmation.AddChild(box);
        box.AddChild(CreateDescription(Text("FULL_UNLOCK.confirmation"), 150f));

        var confirm = _config.CreateButton(Text("FULL_UNLOCK.confirm"), () =>
        {
            confirmation.Hide();
            var result = FullGameUnlock.Apply(confirmedProgress);
            confirmedProgress = null;
            SetStatus(status, Text("FULL_UNLOCK." + result switch
            {
                ProgressEditResult.Changed => "unlocked",
                ProgressEditResult.AlreadyDone => "already_unlocked",
                ProgressEditResult.RunInProgress => "run_in_progress",
                ProgressEditResult.ProfileChanged => "profile_changed",
                _ => "failed",
            }));
            RefreshAscensionChecks();
        });
        confirm.SetColor(new Color("c48235"));
        var cancel = _config.CreateButton(Text("FULL_UNLOCK.cancel"), () =>
        {
            confirmedProgress = null;
            confirmation.Hide();
        });
        var confirmActions = CreateActionRow("FullUnlockConfirmActions");
        confirmActions.AddChild(confirm);
        confirmActions.AddChild(cancel);
        box.AddChild(confirmActions);

        var open = _config.CreateButton(Text("FULL_UNLOCK.button"), () =>
        {
            try
            {
                if (RunManager.Instance.IsInProgress)
                {
                    SetStatus(status, Text("FULL_UNLOCK.run_in_progress"));
                    return;
                }

                confirmedProgress = SaveManager.Instance.Progress;
                SetStatus(status, string.Empty);
                confirmation.Show();
                // 誤操作で解放しないよう、最初はキャンセルにフォーカス
                cancel.GrabFocus();
            }
            catch (Exception ex)
            {
                MainFile.Logger.Warn($"Could not open full unlock confirmation: {ex.Message}");
                SetStatus(status, Text("FULL_UNLOCK.failed"));
            }
        });
        open.Name = "UnlockAllGameContent";
        open.SetColor(new Color("bf8535"));
        open.CustomMinimumSize = new Vector2(620f, 76f);
        open.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

        section.AddChild(open);
        section.AddChild(confirmation);
        section.AddChild(status);
    }
}
