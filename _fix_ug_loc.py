# -*- coding: utf-8 -*-
"""jpn/eng cards.json の UG 連動漏れを一括修正する。"""
from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent

JPN_UPDATES: dict[str, str] = {
    # Priority A
    "HYPNOSISCREATOR-PROSELYTIZE.description":
        "推し活と沼{BogPower:diff()}を付与する。戦闘終了時、{Gold:diff()}ゴールドを獲得する。廃棄。",
    "HYPNOSISCREATOR-GAZE_LIGHT.description":
        "{Block:diff()}ブロックを得る。脱力{WeakPower:diff()}を付与する。",
    "HYPNOSISCREATOR-ABNORMAL_TRANSFORM.description":
        "X枚まで手札のカードをランダムな他色アブノーマルカードに変化させ、このターン、コストなしでプレイできる。\n[gold]UG: ランダムなアップグレード済みの他色アブノーマルカードに変化する。[/gold]",
    "HYPNOSISCREATOR-COLLAR_TRAINING.description":
        "相手を引き寄せる。すでに相手が引き寄せられている場合、破滅{Doom:diff()}を付与してDomSub性癖に目覚めさせる。\n[gold]UG: さらにランダムな調教命令カードを2枚手札に加える。[/gold]",
    # Numeric Priority 0
    "HYPNOSISCREATOR-HC_DEFEND.description":
        "{Block:diff()}ブロックを得る。",
    "HYPNOSISCREATOR-PLEASURE_CYCLE.description":
        "破滅{Doom:diff()}を付与する。対象がトランス状態なら、さらに破滅{Doom:diff()}を付与する。このカードを手札に戻す。",
    "HYPNOSISCREATOR-PREFINGER.description":
        "弱体{VulnerablePower:diff()}と脱力{WeakPower:diff()}を付与し{Energy:diff()}エナジーを得る。",
    "HYPNOSISCREATOR-DRUG_HYPNOSIS.description":
        "カウント。毒{PoisonPower:diff()}、破滅{Doom:diff()}、筋力低下{StrengthLoss:diff()}、弱体{VulnerablePower:diff()}、脱力{WeakPower:diff()}、沼{Bog:diff()}を付与する。",
    "HYPNOSISCREATOR-METRONOME_CARD.description":
        "{Block:diff()}ブロックを得る。次のターン、2枚追加で引く。",
    "HYPNOSISCREATOR-PETTING.description":
        "脆弱{FrailPower:diff()}を付与する。カードを{Cards:diff()}枚引く。",
    "HYPNOSISCREATOR-HYPNOSIS_INTRO.description":
        "{Draw:diff()}枚カードを引く。同名のカードを同じ相手にプレイするたび、引く枚数が1少なくなる。（現在N枚引く）",
    "HYPNOSISCREATOR-CAN_DO_IT.description":
        "脱力{WeakPower:diff()}を付与する。沼{BogPower:diff()}を付与する。",
    "HYPNOSISCREATOR-MENTAL_CARE.description":
        "破滅を除く対象のデバフをすべて解除する。解除したデバフ1種類につき、{BlockPerType:diff()}ブロックを得る。沼{BogPower:diff()}を付与する。",
    "HYPNOSISCREATOR-BOTTOMLESS_BOG.description":
        "ターン終了時、トランスと沼の数値1につき、破滅を{DoomPerStack:diff()}付与する。",
    "HYPNOSISCREATOR-BARE_INSTINCT.description":
        "ターン開始時、手札のランダムなアタック{Enchant:diff()}枚に「本能」をエンチャントする。",
    "HYPNOSISCREATOR-POLYNESIAN_HYPNOSIS.description":
        "カウント。相手すべてに睡眠{AsleepPower:diff()}、沼{Bog:diff()}、トランス1を付与。",
    "HYPNOSISCREATOR-ASMR_HYPNOSIS.description":
        "プレイヤーは「左」「右」に割り振られる。左右交互にプレイするたび、対象に破滅{Doom:diff()}を付与する。",
    "HYPNOSISCREATOR-SLIME_HYPNOSIS.description":
        "カウント。1ターンの間、相手の名前を「スライム」に、行動を「粘液付与{Slimed:diff()}枚」に上書きする。トランス1。",
    "HYPNOSISCREATOR-COGNITIVE_SHUFFLE.description":
        "カウント。トランス2。キャラクターカード3枚からひとつ選び、対応するパワー効果を得る。ターン開始時に対応するカードをランダムに{Cards:diff()}枚生成し、そのターンコストを支払わずにプレイできる。相手がトランス状態中継続する。",
    "HYPNOSISCREATOR-CULT_LEADER.description":
        "すべての性癖カードが相手の性癖に刺さるようになる。トランスを付与するたび、次のターンに1エナジーを得てカードを{Draw:diff()}枚引く。",
    "HYPNOSISCREATOR-PLANT_PARASITE_HYPNOSIS.description":
        "カウント。{Damage:diff()}ダメージを与える。対象は毎ターンHPを{ConstrictPower:diff()}失う。戦闘終了後、追加のレリック報酬を獲得する。トランス1。",
    "HYPNOSISCREATOR-LULLABY_HYPNOSIS.description":
        "カウント。睡眠{AsleepPower:diff()}を与える。対象のHPを{Heal:diff()}回復する。トランス1。",
    "HYPNOSISCREATOR-TENTACLE_RECALL.description":
        "締め付け{ConstrictPower:diff()}を付与する。次のターン、このカードのコピーを手札に加える。",
    "HYPNOSISCREATOR-INFINITE_UPGRADE_STRING.description":
        "カウント。対象はHP{LoseHp:diff()}を失う。トランス1。このカードは何度でもアップグレードできる。",
    "HYPNOSISCREATOR-TRAINING_COMMAND_CARD.description":
        "ランダムな{Cards:diff()}枚のDomSubコマンドカードを手札に加える。",
    "HYPNOSISCREATOR-BABY_HYPNOSIS.description":
        "カウント。対象のバフをすべて解除し、縮小{ShrinkPower:diff()}を付与する。トランス1。",
    "HYPNOSISCREATOR-WORD_FLOOD.description":
        "催眠系カウントカードをプレイした時、カードを{Draw:diff()}枚引く。",
    "HYPNOSISCREATOR-FETISH_UNDERSTANDING.description":
        "性癖カードをプレイするたび、{Block:diff()}ブロックを得る。",
    "HYPNOSISCREATOR-SELF_SUGGESTION.description":
        "筋力{StrengthPower:diff()}を得る。敏捷{DexterityPower:diff()}を得る。この戦闘中、ランダムな心臓レリックを3つ獲得する。",
    "HYPNOSISCREATOR-WAIT_A_BIT_MORE.description":
        "催眠系カウントカードを{Cards:diff()}枚選ぶ。そのコストを初期値に戻し、リプレイ1を付与する。",
    "HYPNOSISCREATOR-MARSHMALLOW_ANSWER.description":
        "ランダムな呪いカード{Cards:diff()}枚を手札に追加する。相手が攻撃を予定している場合、ブロック39に上書きし沼2を付与する。",
    "HYPNOSISCREATOR-FETISH_PARTY.description":
        "味方プレイヤーすべての手札にランダムな性癖カード{Cards:diff()}枚を加える。",
    "HYPNOSISCREATOR-FREE_HUG.description":
        "相手を引き寄せる。すでに相手が引き寄せられている場合、破滅{Doom:diff()}と沼1を与え、ランダムな味方に2枚パスする。",
    "HYPNOSISCREATOR-BEGINNER_HYPNOSIS.description":
        "破滅12を付与する。対象にプレイする次の{PlantCards:diff()}枚の性癖カードの性癖を相手に植え付ける。トランス1。",
    # Qualitative UG tips
    "HYPNOSISCREATOR-AS_YOU_WISH.description":
        "性癖カードをプレイするたび、対応する効果を得る。アブノーマル：筋力1を得る。SM：活力2を得る。DomSub：ブロック1を得る。\n[gold]UG: アブノーマルは筋力1と敏捷1、SMは活力3、DomSubはブロック2。[/gold]",
    "HYPNOSISCREATOR-RAPPORT.description":
        "手札のカウントを1つ進める。ターン開始時、前のターンに相手を攻撃していなかった場合に手札のカウントがひとつ追加で進む。\n[gold]UG: ターン開始時、条件なしで手札のカウントがひとつ追加で進む。[/gold]",
    "HYPNOSISCREATOR-FINGER_SNAP.description":
        "トランス1。破滅5を付与する。\n[gold]UG: 保留。[/gold]",
    "HYPNOSISCREATOR-HARMONY.description":
        "相手の攻撃と同値のブロックを得る。\n[gold]UG: 廃棄が消える。[/gold]",
    "HYPNOSISCREATOR-MIRRORING.description":
        "相手の攻撃予定と同じ攻撃を行う。\n[gold]UG: 廃棄が消える。[/gold]",
    "HYPNOSISCREATOR-DEEP_BREATH.description":
        "カードを1枚引く。次のターンエナジー1を得る。\n[gold]UG: 廃棄が消える。[/gold]",
    "HYPNOSISCREATOR-BREATH_CONTROL.description":
        "このターン、カードをプレイするたび相手は筋力1を失う。相手の攻撃値が0点になった時、スタンする。\n[gold]UG: 保留。[/gold]",
    "HYPNOSISCREATOR-ZERO_OUT.description":
        "カウント。相手の攻撃値を0にする。トランス1。\n[gold]UG: 攻撃値とブロックを0にする。[/gold]",
    "HYPNOSISCREATOR-SOFT_TECHNIQUE.description":
        "デバフを1種類解除する。\n[gold]UG: デバフすべてを解除する。[/gold]",
    "HYPNOSISCREATOR-CONTINUOUS_TRANCE.description":
        "トランス1を3回付与し、性癖に「トランス」を追加する。\n[gold]UG: 相手すべてに適用する。[/gold]",
    "HYPNOSISCREATOR-CATALEPSY.description":
        "相手にスロー1を付与する。\n[gold]UG: 相手がトランス時はスローの蓄積量がリセットされない。[/gold]",
    "HYPNOSISCREATOR-CORROSION.description":
        "アタックをプレイするたび、ランダムな催眠系カウントカードを手札に1枚加える。\n[gold]UG: 相手の性癖に該当するカードが優先して生成される。[/gold]",
    "HYPNOSISCREATOR-SUGGESTION_RELEASE.description":
        "0ダメージを与える。相手のトランス状態を解除し、その数値に応じたエナジーを獲得する。\n[gold]UG: 同じ数だけカードも引く。[/gold]",
    "HYPNOSISCREATOR-ALL_IN_ONE.description":
        "カウント。すべての催眠系カウントカードを対象にプレイする。トランス1。\n[gold]UG: 廃棄されたものも含める。[/gold]",
    "HYPNOSISCREATOR-KNOW_IT_ALL.description":
        "性癖を刺した時の効果が2倍になる。\n[gold]UG: 天賦。[/gold]",
    "HYPNOSISCREATOR-OVER_FOCUS.description":
        "手札にあるスキルカード1枚につき、エナジー1を得る。\n[gold]UG: 廃棄が消える。[/gold]",
    "HYPNOSISCREATOR-MASS_HYPNOSIS.description":
        "催眠系カウントカードの対象が相手すべてになる。\n[gold]UG: 天賦。[/gold]",
    "HYPNOSISCREATOR-STATUS_HYPNOSIS.description":
        "トランス状態の相手に、状態異常と呪いカードをプレイできるようになる。\n[gold]UG: 天賦。[/gold]",
    "HYPNOSISCREATOR-HEART_CRAVING.description":
        "保有しているすべての心臓レリックがこの戦闘中、再使用可能状態になる。\n[gold]UG: すべての心臓レリックが再使用可能状態になる。[/gold]",
    "HYPNOSISCREATOR-HEARTBEAT_SHARE.description":
        "保有している心臓レリックから得た効果を味方プレイヤー1人に共有する。\n[gold]UG: すべてに共有する。[/gold]",
    "HYPNOSISCREATOR-LOVE_HYPNOSIS.description":
        "カウント。相手がバフ行動を予定している場合、その対象をプレイヤーに変更する。トランス1。\n[gold]UG: バフまたはブロック行動も対象。[/gold]",
    "HYPNOSISCREATOR-CARDIAC_ARREST_HYPNOSIS.description":
        "カウント。3ターン後、相手の心臓が止まる。ボスの場合は2倍のターン数になる。\n[gold]UG: 心停止時に追加のレリック報酬を獲得する。[/gold]",
    "HYPNOSISCREATOR-SENSITIVITY3000.description":
        "トランス3。必ず性癖に刺さる。このターン、相手の受けるダメージを3.000倍にする。\n[gold]UG: コスト1。[/gold]",
    "HYPNOSISCREATOR-FINGER_COUNT.description":
        "手札のカウントカードすべてのコストを1下げる。\n[gold]UG: コスト0。[/gold]",
    "HYPNOSISCREATOR-SOFTEN.description":
        "トランス状態の相手が与えるダメージが、スタック数1につき20%低下する。\n[gold]UG: コスト1。[/gold]",
    "HYPNOSISCREATOR-SENSE_SHARE.description":
        "このターン中、単体へのアタックカードは全体が対象になる。\n[gold]UG: コスト0。[/gold]",
    "HYPNOSISCREATOR-ERICKSONIAN.description":
        "相手の性癖を刺した時、手札のカードのカウントがひとつ進む。\n[gold]UG: コスト0。[/gold]",
    "HYPNOSISCREATOR-RITUAL_REVEAL.description":
        "催眠系カウントカードを山札からランダムに2枚、手札に加える。1枚は相手の性癖に合致するカードが優先して選ばれる。\n[gold]UG: コスト0。[/gold]",
    "HYPNOSISCREATOR-ZERO_SHORTCUT.description":
        "3ブロックを得る。これを1数値を減らして0ブロックを得るまで繰り返す。手札の催眠系カウントカードのコストを0にする。（Nブロックを得る）\n[gold]UG: コスト2。[/gold]",
    "HYPNOSISCREATOR-MASTERY.description":
        "プレイした催眠系カウントカードすべてを戦闘終了後にアップグレードする。\n[gold]UG: コスト1。[/gold]",
    "HYPNOSISCREATOR-AMBUSH_HYPNOSIS.description":
        "敵の数だけドローする。引いたカードをランダムな相手にプレイする。\n[gold]UG: コスト1。[/gold]",
    "HYPNOSISCREATOR-ACCEPTANCE_NEED.description":
        "ダメージを受けた回数分、次のターンにエナジーを得てカードを引く。\n[gold]UG: コスト1。[/gold]",
    "HYPNOSISCREATOR-ENCORE.description":
        "プレイした催眠系カウントカードは、コストが初期値にリセットされて手札に戻る。\n[gold]UG: コスト0。[/gold]",
    "HYPNOSISCREATOR-HUNDRED_EIGHT.description":
        "すべての相手を、すべての性癖に目覚めさせる。プレイ後、コストが1増加する。3コストのこのカードをプレイした時、すべての敵に1ダメージを108回与えて廃棄される。\n[gold]UG: プレイ後は山札に入る。[/gold]",
    "HYPNOSISCREATOR-SPANKING.description":
        "{Damage:diff()}ダメージを2回与える。性癖に刺さった時、もう一度プレイできる。\n[gold]UG: リプレイ2。[/gold]",
}

ENG_UPDATES: dict[str, str] = {
    "HYPNOSISCREATOR-GAZE_LIGHT.description":
        "Gain {Block:diff()} [gold]Block[/gold].\nApply {WeakPower:diff()} Weak.",
    "HYPNOSISCREATOR-ABNORMAL_TRANSFORM.description":
        "Choose up to X cards in your hand. Transform them into random Abnormal Fetish cards for this combat only, with 0 cost.\n[gold]Upgrade: transform into upgraded Abnormal cards.[/gold]",
    "HYPNOSISCREATOR-COLLAR_TRAINING.description":
        "Pull the target. If they were already Pulled, apply {Doom:diff()} Doom and awaken DomSub.\n[gold]Upgrade: also add 2 Training Command cards when Doom is applied.[/gold]",
}


def apply(path: Path, updates: dict[str, str]) -> int:
    data = json.loads(path.read_text(encoding="utf-8"))
    n = 0
    for k, v in updates.items():
        if k not in data:
            print("MISSING KEY", path.name, k)
            continue
        if data[k] != v:
            data[k] = v
            n += 1
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return n


def main() -> None:
    jn = apply(ROOT / "HypnosisCreator/localization/jpn/cards.json", JPN_UPDATES)
    en = apply(ROOT / "HypnosisCreator/localization/eng/cards.json", ENG_UPDATES)
    print(f"updated jpn={jn} eng={en}")


if __name__ == "__main__":
    main()
