using BaseLib.Patches.Localization;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// UGの定性差分を説明文へ緑表示する。全文差し替えはしない（キーワード色・性癖行を消さない）。
/// </summary>
public static class UpgradeDescriptionHooks
{
    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += Apply;

    private static void Apply(CardModel card, Creature? target, ref string description)
    {
        // --- 既存（心停止・足蹴など） ---
        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is CardiacArrestHypnosis,
            "相手の心臓が止まる。",
            "相手の心臓が止まり[green]追加のレリック報酬を獲得する[/green]。",
            "their heart stops.",
            "their heart stops and [green]you gain an extra Relic reward[/green].");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is PlantParasiteHypnosis,
            "戦闘終了後、追加のレリック報酬を獲得する。",
            "At end of combat, gain an extra Relic reward.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is Kick,
            "プレイ後、山札に入る。",
            "After play, shuffle this into your draw pile.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is HundredEight,
            "プレイ後は山札に入る。",
            "After play, shuffle this into your draw pile.");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is AbnormalTransform,
            "ランダムなアブノーマル系名称",
            "ランダムな[green]アップグレード済み[/green]アブノーマル系名称",
            "random Abnormal-named",
            "random [green]upgraded[/green] Abnormal-named");

        // --- 定性UG（水平展開） ---
        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is SoftTechnique,
            "デバフを1種類解除する。",
            "デバフを[green]すべて[/green]解除する。",
            "Remove 1 random debuff from yourself.",
            "Remove [green]all[/green] debuffs from yourself.");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is ZeroOut,
            "相手の攻撃値を0にする。",
            "相手の攻撃値を0にし、[green]ブロックも0にする[/green]。",
            "Set their attack value to 0.",
            "Set their attack value to 0 and [green]remove all Block[/green].");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is ContinuousTrance,
            "相手すべてに同じ効果を与える。",
            "Apply the same effect to all opponents.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is LoveHypnosis,
            "ブロック行動の対象もプレイヤーに変更する。",
            "Also redirect Block intents to you.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is BrainSlimeHypnosis,
            "相手すべてに同じ効果を与える。",
            "Apply the same effect to all opponents.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is AllInOne,
            "廃棄山のカードも含める。",
            "Include cards in your Exhaust pile.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is Corrosion,
            "相手の性癖に該当するカードを優先して生成する。",
            "Prefer Count cards matching the Attack's fetish.");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is HeartCraving,
            "この戦闘中、再使用可能状態になる。",
            "[green]再使用可能状態になる[/green]。",
            "become reusable this combat.",
            "[green]become reusable[/green].");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is HeartbeatShare,
            "味方プレイヤー1人に共有する。",
            "味方プレイヤー[green]すべて[/green]に共有する。",
            "with one ally.",
            "with [green]all allies[/green].");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is CollarTraining,
            "すでに引き寄せられている場合、ランダムな調教命令を2枚手札に加える。",
            "If already Pulled, add 2 random Training Command cards to your hand.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is SuggestionRelease,
            "解除した数値と同じ枚数のカードを引く。",
            "Also draw cards equal to the Trance removed.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is Catalepsy,
            "相手がトランス時はスローの蓄積量がリセットされない。",
            "While Tranced, Slow stacks do not reset.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is Stare,
            "手札のカウントを1つ進める。",
            "Advance Count costs in your hand by 1.");

        UpgradeCardText.AppendGreenLine(card, ref description, c => c is Whisper,
            "必ず性癖に刺さる。",
            "This card always hits Fetish.");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is Spanking,
            "もう一度プレイできる。",
            "[green]さらに2回[/green]プレイできる。",
            "play this card again.",
            "play this card [green]2 more times[/green].");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is Rapport,
            "ターン開始時、前のターンに相手を攻撃していなかった場合に手札のカウントがひとつ追加で進む。",
            "ターン開始時、[green]手札のカウントがひとつ追加で進む[/green]。",
            "If you didn't attack an opponent last turn, reduce it by 1 more.",
            "[green]At the start of your turn, reduce Count costs in your hand by 1 more.[/green]");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is AsYouWish,
            "アブノーマル：筋力1を得る。SM：活力2を得る。DomSub：ブロック1を得る。",
            "アブノーマル：筋力1と[green]敏捷1[/green]を得る。SM：活力[green]3[/green]を得る。DomSub：ブロック[green]2[/green]を得る。",
            "Abnormal grants Strength, SM grants Vigor, DomSub grants Block.",
            "Abnormal grants Strength and [green]Dexterity[/green], SM grants [green]3[/green] Vigor, DomSub grants [green]2[/green] Block.");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is AmbushHypnosis,
            "相手の数だけカードを引く",
            "相手の数[green]+1[/green]枚カードを引く",
            "Draw cards equal to the number of opponents",
            "Draw cards equal to the number of opponents [green]+ 1[/green]");

        // 廃棄は CanonicalKeywords の自動挿入に任せる（説明文に書くと「廃棄。廃棄。」になる）
        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is CheerFromTheAbyss,
            "増加する。",
            "増加し、[green]次のターン開始時にあらゆる場所から手札に加える[/green]。",
            "in your deck by {Increase:diff()}.",
            "in your deck by {Increase:diff()} and [green]put it into your hand from anywhere at the start of your next turn[/green].");
    }
}
