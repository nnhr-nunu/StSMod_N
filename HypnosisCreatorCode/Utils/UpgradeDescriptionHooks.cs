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

        // 心臓えぐり出し+: リーサル → 破滅とどめ（通常戦闘のみ）。ダメージ強化なし。
        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is HeartGouge,
            "リーサル時、追加のレリック報酬を獲得する。",
            "[green]対象の[gold]破滅[/gold]が残りHPの[blue]50%[/blue]以上の場合、とどめを刺し追加のレリック報酬を獲得する。とどめは通常戦闘でのみ発生する。[/green]",
            "If Fatal, gain an extra Relic reward.",
            "[green]If the target's [gold]Doom[/gold] is at least [blue]50%[/blue] of their remaining HP, Execute them and gain an extra Relic reward. Execute only occurs in normal combats.[/green]");

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
            "デバフを[blue]1[/blue]種類解除する。",
            "デバフを[green]すべて[/green]解除する。",
            "Remove [blue]1[/blue] debuff.",
            "Remove [green]all[/green] debuffs.");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is ZeroOut,
            "相手の攻撃値を[blue]0[/blue]にする。",
            "相手の攻撃値を[blue]0[/blue]にし、[green]ブロックも0にする[/green]。",
            "Set their attack value to [blue]0[/blue].",
            "Set their attack value to [blue]0[/blue] and [green]remove all Block[/green].");
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
            "この戦闘中に限り再使用可能状態になる。",
            "再使用可能状態になる（[green]戦闘後も残る[/green]）。",
            "become reusable for this combat only.",
            "become reusable ([green]persists after combat[/green]).");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is HeartbeatShare,
            "味方プレイヤー[blue]1[/blue]人と共有して発動する。",
            "味方プレイヤー[green]すべて[/green]と共有して発動する。",
            "shared with [blue]1[/blue] ally player.",
            "shared with [green]all[/green] ally players.");
        UpgradeCardText.AppendGreenLine(card, ref description, c => c is CollarTraining,
            "すでに引き寄せられている場合、ランダムな調教命令を2枚手札に加える。",
            "If already Pulled, add 2 random Training Command cards to your hand.");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is SuggestionRelease,
            "その数値に応じて{energyPrefix:energyIcons(1)}を獲得し",
            "その数値の[green]2[/green]倍の{energyPrefix:energyIcons(1)}を獲得し",
            "Gain {energyPrefix:energyIcons(1)} equal to the amount removed and",
            "Gain [green]twice[/green] as much {energyPrefix:energyIcons(1)} as the amount removed and");

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
            "you may play this again.",
            "you may play this [green]2 more times[/green].");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is Rapport,
            "ターン開始時、前のターンに相手を攻撃していなかった場合に[gold]手札[/gold]の[gold]カウント[/gold]がひとつ追加で進む。",
            "ターン開始時、[green][gold]手札[/gold]の[gold]カウント[/gold]がひとつ追加で進む[/green]。",
            "At the start of your turn, if you did not attack an opponent last turn, advance [gold]Count[/gold] in your [gold]Hand[/gold] once more.",
            "[green]At the start of your turn, advance [gold]Count[/gold] in your [gold]Hand[/gold] once more.[/green]");

        UpgradeCardText.ReplaceWhenUpgraded(card, ref description, c => c is AsYouWish,
            "アブノーマル：[gold]筋力[/gold][blue]1[/blue]（強化時はさらに[gold]敏捷[/gold][blue]1[/blue]）。SM：[gold]活力[/gold][blue]2[/blue]。DomSub：[blue]1[/blue][gold]ブロック[/gold]。",
            "アブノーマル：[gold]筋力[/gold][blue]1[/blue]と[green][gold]敏捷[/gold][blue]1[/blue][/green]。SM：[gold]活力[/gold][green][blue]3[/blue][/green]。DomSub：[green][blue]2[/blue][/green][gold]ブロック[/gold]。",
            "Abnormal grants [blue]1[/blue] [gold]Strength[/gold] (and [blue]1[/blue] [gold]Dexterity[/gold] if upgraded), SM grants [gold]Vigor[/gold], DomSub grants [gold]Block[/gold].",
            "Abnormal grants [blue]1[/blue] [gold]Strength[/gold] and [green][blue]1[/blue] [gold]Dexterity[/gold][/green], SM grants [green][blue]3[/blue][/green] [gold]Vigor[/gold], DomSub grants [green][blue]2[/blue][/green] [gold]Block[/gold].");
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
