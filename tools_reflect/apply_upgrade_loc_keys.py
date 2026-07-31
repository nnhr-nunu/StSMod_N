# -*- coding: utf-8 -*-
"""Merge upgradeAppend / upgradeReplaceFrom / upgradeReplaceTo into cards.json (8 langs)."""
import json
import pathlib

ROOT = pathlib.Path(__file__).resolve().parents[1]
LANGS = ["jpn", "eng", "kor", "deu", "spa", "rus", "zhs", "zht"]

# upgradeAppend: appended as full green line by UpgradeCardText
APPEND = {
    "HYPNOSISCREATOR-HEART_GOUGE": {
        "jpn": "さらに、対象の[gold]破滅[/gold]が残りHPの[blue]50%[/blue]以上なら破滅とどめを刺す。破滅とどめは通常戦闘でのみ発生する。",
        "eng": "Additionally, if the target's [gold]Doom[/gold] is at least [blue]50%[/blue] of their remaining HP, apply a [gold]Doom Execute[/gold]. [gold]Doom Execute[/gold] only occurs in normal combats.",
        "kor": "추가로, 대상의 [gold]파멸[/gold]이 남은 HP의 [blue]50%[/blue] 이상이면 파멸 처형을 가한다. 파멸 처형은 일반 전투에서만 발생한다.",
        "deu": "Zusätzlich: Liegt das [gold]Verhängnis[/gold] des Ziels bei mindestens [blue]50%[/blue] seiner verbleibenden LP, führe [gold]Verhängnis-Vollstreckung[/gold] aus. [gold]Verhängnis-Vollstreckung[/gold] tritt nur in normalen Kämpfen auf.",
        "spa": "Además, si la [gold]Perdición[/gold] del objetivo es al menos el [blue]50%[/blue] de su HP restante, aplica [gold]Ejecución por Perdición[/gold]. [gold]Ejecución por Perdición[/gold] solo ocurre en combates normales.",
        "rus": "Дополнительно: если [gold]Рок[/gold] цели составляет не менее [blue]50%[/blue] оставшегося HP, примените [gold]Казнь Роком[/gold]. [gold]Казнь Роком[/gold] происходит только в обычных боях.",
        "zhs": "此外，若对象的[gold]破灭[/gold]不低于剩余HP的[blue]50%[/blue]，则施加[gold]破灭处决[/gold]。[gold]破灭处决[/gold]仅在普通战斗中发生。",
        "zht": "此外，若對象的[gold]破滅[/gold]不低於剩餘HP的[blue]50%[/blue]，則施加[gold]破滅處決[/gold]。[gold]破滅處決[/gold]僅在普通戰鬥中發生。",
    },
    "HYPNOSISCREATOR-KICK": {
        "jpn": "プレイ後、山札に入る。",
        "eng": "After play, shuffle this into your draw pile.",
        "kor": "사용 후, 뽑을 더미에 섞어 넣는다.",
        "deu": "Nach dem Ausspielen wird diese Karte ins Ziehstapel gemischt.",
        "spa": "Tras jugarla, barájala en tu pila de robo.",
        "rus": "После розыгрыша перемешайте эту карту в колоду добора.",
        "zhs": "打出后，洗入抽牌堆。",
        "zht": "打出後，洗入抽牌堆。",
    },
    "HYPNOSISCREATOR-HUNDRED_EIGHT": {
        "jpn": "プレイ後は山札に入る。",
        "eng": "After play, shuffle this into your draw pile.",
        "kor": "사용 후, 뽑을 더미에 섞어 넣는다.",
        "deu": "Nach dem Ausspielen wird diese Karte ins Ziehstapel gemischt.",
        "spa": "Tras jugarla, barájala en tu pila de robo.",
        "rus": "После розыгрыша перемешайте эту карту в колоду добора.",
        "zhs": "打出后，洗入抽牌堆。",
        "zht": "打出後，洗入抽牌堆。",
    },
    "HYPNOSISCREATOR-CONTINUOUS_TRANCE": {
        "jpn": "相手すべてに同じ効果を与える。",
        "eng": "Apply the same effect to all opponents.",
        "kor": "모든 상대에게 같은 효과를 부여한다.",
        "deu": "Wende denselben Effekt auf alle Gegner an.",
        "spa": "Aplica el mismo efecto a todos los enemigos.",
        "rus": "Примените тот же эффект ко всем противникам.",
        "zhs": "对所有对手施加相同效果。",
        "zht": "對所有對手施加相同效果。",
    },
    "HYPNOSISCREATOR-LOVE_HYPNOSIS": {
        "jpn": "ブロック行動の対象もプレイヤーに変更する。",
        "eng": "Also redirect Block intents to you.",
        "kor": "블록 행동의 대상도 플레이어로 변경한다.",
        "deu": "Lenke auch Block-Absichten auf dich um.",
        "spa": "También redirige las intenciones de Bloqueo hacia ti.",
        "rus": "Также перенаправляйте намерения Блока на вас.",
        "zhs": "也将格挡行动的目标改为玩家。",
        "zht": "也將格擋行動的目標改為玩家。",
    },
    "HYPNOSISCREATOR-BRAIN_SLIME_HYPNOSIS": {
        "jpn": "相手すべてに同じ効果を与える。",
        "eng": "Apply the same effect to all opponents.",
        "kor": "모든 상대에게 같은 효과를 부여한다.",
        "deu": "Wende denselben Effekt auf alle Gegner an.",
        "spa": "Aplica el mismo efecto a todos los enemigos.",
        "rus": "Примените тот же эффект ко всем противникам.",
        "zhs": "对所有对手施加相同效果。",
        "zht": "對所有對手施加相同效果。",
    },
    "HYPNOSISCREATOR-ALL_IN_ONE": {
        "jpn": "廃棄山のカードも含める。",
        "eng": "Include cards in your Exhaust pile.",
        "kor": "소진 더미의 카드도 포함한다.",
        "deu": "Schließe Karten im Erschöpfungsstapel ein.",
        "spa": "Incluye las cartas de tu pila de agotamiento.",
        "rus": "Включая карты в стопке истощения.",
        "zhs": "包含消耗堆中的卡牌。",
        "zht": "包含消耗堆中的卡牌。",
    },
    "HYPNOSISCREATOR-CORROSION": {
        "jpn": "相手の性癖に該当するカードを優先して生成する。",
        "eng": "Prefer Count cards matching the opponent's Fetish.",
        "kor": "상대의 페티시에 해당하는 카드를 우선 생성한다.",
        "deu": "Bevorzuge Countdown-Karten, die zum Fetisch des Gegners passen.",
        "spa": "Prioriza cartas de Cuenta que coincidan con el fetiche del enemigo.",
        "rus": "Предпочитайте карты Счётчика, соответствующие фетишу противника.",
        "zhs": "优先生成符合对手性癖的计数卡。",
        "zht": "優先生成符合對手性癖的計數卡。",
    },
    "HYPNOSISCREATOR-COLLAR_TRAINING": {
        "jpn": "すでに引き寄せられている場合、ランダムな調教命令を2枚手札に加える。",
        "eng": "If already Pulled, add 2 random Training Command cards to your hand.",
        "kor": "이미 끌어당겨진 경우, 무작위 조교 명령 카드 2장을 손에 추가한다.",
        "deu": "Wenn bereits gezogen, füge 2 zufällige Trainingsbefehl-Karten der Hand hinzu.",
        "spa": "Si ya está atraído, añade 2 cartas de Orden de adiestramiento aleatorias a la mano.",
        "rus": "Если цель уже притянута, добавьте 2 случайные карты Приказа дрессировки в руку.",
        "zhs": "若已被拉近，将2张随机调教命令卡加入手牌。",
        "zht": "若已被拉近，將2張隨機調教命令卡加入手牌。",
    },
    "HYPNOSISCREATOR-CATALEPSY": {
        "jpn": "相手がトランス時はスローの蓄積量がリセットされない。",
        "eng": "While Tranced, Slow stacks do not reset.",
        "kor": "상대가 트랜스 상태일 때 슬로우 누적량이 초기화되지 않는다.",
        "deu": "Während Trance setzt sich Langsam nicht zurück.",
        "spa": "Mientras esté en Trance, las acumulaciones de Lentitud no se reinician.",
        "rus": "Пока цель в Трансе, стаки Замедления не сбрасываются.",
        "zhs": "对手处于恍惚时，迟缓层数不会重置。",
        "zht": "對手處於恍惚時，遲緩層數不會重置。",
    },
    "HYPNOSISCREATOR-STARE": {
        "jpn": "手札のカウントを1つ進める。",
        "eng": "Advance Count costs in your hand by 1.",
        "kor": "손에 있는 카운트를 1 진행시킨다.",
        "deu": "Bringe den Countdown einer Karte in der Hand um 1 voran.",
        "spa": "Avanza la Cuenta de una carta en la mano en 1.",
        "rus": "Продвиньте Счётчик карты в руке на 1.",
        "zhs": "使手牌中的计数推进1次。",
        "zht": "使手牌中的計數推進1次。",
    },
    "HYPNOSISCREATOR-FINGER_SNAP": {
        "jpn": "[gold]手札[/gold]の[gold]カウント[/gold]カードすべてのコストを[blue]1[/blue]下げる。",
        "eng": "Reduce the cost of all [gold]Count[/gold] cards in your [gold]Hand[/gold] by [blue]1[/blue].",
        "kor": "[gold]손[/gold]에 있는 모든 [gold]카운트[/gold] 카드의 코스트를 [blue]1[/blue] 낮춘다.",
        "deu": "Senke die Kosten aller [gold]Countdown[/gold]-Karten in der [gold]Hand[/gold] um [blue]1[/blue].",
        "spa": "Reduce el coste de todas las cartas de [gold]Cuenta[/gold] en la [gold]mano[/gold] en [blue]1[/blue].",
        "rus": "Уменьшите стоимость всех карт [gold]Счётчика[/gold] в [gold]руке[/gold] на [blue]1[/blue].",
        "zhs": "使[gold]手牌[/gold]中所有[gold]计数[/gold]卡的费用降低[blue]1[/blue]。",
        "zht": "使[gold]手牌[/gold]中所有[gold]計數[/gold]卡的費用降低[blue]1[/blue]。",
    },
    "HYPNOSISCREATOR-SENSE_SHARE": {
        "jpn": "さらに、自分が受けたダメージを相手全体へ伝播させる。",
        "eng": "Additionally, propagate damage you receive to ALL opponents.",
        "kor": "추가로, 자신이 받은 피해를 모든 상대에게 전파한다.",
        "deu": "Zusätzlich wird erlittener Schaden auf ALLE Gegner übertragen.",
        "spa": "Además, propaga el daño que recibes a TODOS los enemigos.",
        "rus": "Дополнительно распространяйте получаемый урон на ВСЕХ противников.",
        "zhs": "此外，将自身受到的伤害传播给所有对手。",
        "zht": "此外，將自身受到的傷害傳播給所有對手。",
    },
    "HYPNOSISCREATOR-WHISPER": {
        "jpn": "このカードは必ず性癖に刺さる。",
        "eng": "This card always hits Fetish.",
        "kor": "이 카드는 반드시 페티시에 적중한다.",
        "deu": "Diese Karte trifft immer den Fetisch.",
        "spa": "Esta carta siempre acierta el fetiche.",
        "rus": "Эта карта всегда попадает в фетиш.",
        "zhs": "此卡必定刺中性癖。",
        "zht": "此卡必定刺中性癖。",
    },
    "HYPNOSISCREATOR-ABDOMINAL_STRIKE": {
        "jpn": "[gold]無慈悲[/gold]を得る。",
        "eng": "Gain [gold]Cruelty[/gold].",
        "kor": "[gold]무자비[/gold]를 얻는다.",
        "deu": "Erhalte [gold]Grausamkeit[/gold].",
        "spa": "Obtén [gold]Crueldad[/gold].",
        "rus": "Получите [gold]Жестокость[/gold].",
        "zhs": "获得[gold]无情[/gold]。",
        "zht": "獲得[gold]無情[/gold]。",
    },
    "HYPNOSISCREATOR-CHEER_FROM_THE_ABYSS": {
        "jpn": "次のターン開始時にあらゆる場所から手札に加える。",
        "eng": "At the start of your next turn, put it into your Hand from anywhere.",
        "kor": "다음 턴 시작 시, 어디에서든 손에 넣는다.",
        "deu": "Zu Beginn deines nächsten Zuges nimm sie von überall auf die Hand.",
        "spa": "Al inicio de tu próximo turno, ponla en la mano desde cualquier lugar.",
        "rus": "В начале следующего хода возьмите её в руку откуда угодно.",
        "zhs": "下回合开始时，从任意位置加入手牌。",
        "zht": "下回合開始時，從任意位置加入手牌。",
    },
}

REPLACE = {
    "HYPNOSISCREATOR-CARDIAC_ARREST_HYPNOSIS": {
        "jpn": (
            "相手の心臓が止まる。",
            "相手の心臓が止まり[green]追加のレリック報酬を獲得する[/green]。",
        ),
        "eng": (
            "their heart stops.",
            "their heart stops and [green]you gain an extra Relic reward[/green].",
        ),
        "kor": (
            "상대의 심장이 멈춘다.",
            "상대의 심장이 멈추고 [green]추가 유물 보상을 획득한다[/green].",
        ),
        "deu": (
            "bleibt das Herz des Ziels stehen.",
            "bleibt das Herz des Ziels stehen und [green]du erhältst eine zusätzliche Reliktbelohnung[/green].",
        ),
        "spa": (
            "el corazón del objetivo se detiene.",
            "el corazón del objetivo se detiene y [green]obtienes una recompensa de reliquia adicional[/green].",
        ),
        "rus": (
            "сердце цели остановится.",
            "сердце цели остановится, и [green]вы получите дополнительную награду-реликвию[/green].",
        ),
        "zhs": (
            "对手心脏停止。",
            "对手心脏停止，[green]获得额外遗物奖励[/green]。",
        ),
        "zht": (
            "對手心臟停止。",
            "對手心臟停止，[green]獲得額外遺物獎勵[/green]。",
        ),
    },
    "HYPNOSISCREATOR-ABNORMAL_TRANSFORM": {
        "jpn": (
            "ランダムなアブノーマル系名称",
            "ランダムな[green]アップグレード済み[/green]アブノーマル系名称",
        ),
        "eng": (
            "random Abnormal-named",
            "random [green]upgraded[/green] Abnormal-named",
        ),
        "kor": ("무작위 어브노멀 계열 이름", "무작위 [green]강화된[/green] 어브노멀 계열 이름"),
        "deu": ("zufällige Karten aus der „Abnormal“-Reihe", "zufällige [green]verbesserte[/green] Karten aus der „Abnormal“-Reihe"),
        "spa": ("cartas aleatorias de la línea «Anormal»", "cartas aleatorias [green]mejoradas[/green] de la línea «Anormal»"),
        "rus": ("случайные карты из линейки «Отклонение»", "случайные [green]улучшенные[/green] карты из линейки «Отклонение»"),
        "zhs": ("随机的异常系名称", "随机的[green]已升级[/green]异常系名称"),
        "zht": ("隨機的異常系名稱", "隨機的[green]已升級[/green]異常系名稱"),
    },
    "HYPNOSISCREATOR-SOFT_TECHNIQUE": {
        "jpn": ("デバフを[blue]1[/blue]種類解除する。", "デバフを[green]すべて[/green]解除する。"),
        "eng": ("Remove [blue]1[/blue] debuff.", "Remove [green]all[/green] debuffs."),
        "kor": ("약화 [blue]1[/blue]종류를 해제한다.", "약화를 [green]모두[/green] 해제한다."),
        "deu": ("Entferne [blue]1[/blue] Art von Schwächung.", "Entferne [green]alle[/green] Schwächungen."),
        "spa": ("Elimina [blue]1[/blue] tipo de debilitación.", "Elimina [green]todas[/green] las debilitaciones."),
        "rus": ("Снимите [blue]1[/blue] вид дебаффа.", "Снимите [green]все[/green] дебаффы."),
        "zhs": ("解除[blue]1[/blue]种削弱。", "解除[green]所有[/green]削弱。"),
        "zht": ("解除[blue]1[/blue]種削弱。", "解除[green]所有[/green]削弱。"),
    },
    "HYPNOSISCREATOR-ZERO_OUT": {
        "jpn": (
            "相手の攻撃値を[blue]0[/blue]にする。",
            "相手の攻撃値を[blue]0[/blue]にし、[green]ブロックも0にする[/green]。",
        ),
        "eng": (
            "Set their attack value to [blue]0[/blue].",
            "Set their attack value to [blue]0[/blue] and [green]remove all Block[/green].",
        ),
        "kor": (
            "상대의 공격력을 [blue]0[/blue]으로 만든다.",
            "상대의 공격력을 [blue]0[/blue]으로 만들고, [green]방어도도 0으로 만든다[/green].",
        ),
        "deu": (
            "Senkt den Angriffswert des Gegners auf [blue]0[/blue].",
            "Senkt den Angriffswert des Gegners auf [blue]0[/blue] und [green]entferne allen Block[/green].",
        ),
        "spa": (
            "Reduce el valor de ataque del enemigo a [blue]0[/blue].",
            "Reduce el valor de ataque del enemigo a [blue]0[/blue] y [green]elimina todo el Bloqueo[/green].",
        ),
        "rus": (
            "Снижает силу атаки противника до [blue]0[/blue].",
            "Снижает силу атаки противника до [blue]0[/blue] и [green]убирает весь Блок[/green].",
        ),
        "zhs": (
            "将对手的攻击值变为[blue]0[/blue]。",
            "将对手的攻击值变为[blue]0[/blue]，[green]并将格挡也变为0[/green]。",
        ),
        "zht": (
            "將對手的攻擊值變為[blue]0[/blue]。",
            "將對手的攻擊值變為[blue]0[/blue]，[green]並將格擋也變為0[/green]。",
        ),
    },
    "HYPNOSISCREATOR-HEART_CRAVING": {
        "jpn": (
            "この戦闘中に限り再使用可能状態になる。",
            "再使用可能状態になる（[green]戦闘後も残る[/green]）。",
        ),
        "eng": (
            "become reusable for this combat only.",
            "become reusable ([green]persists after combat[/green]).",
        ),
        "kor": (
            "이번 전투 중에만 다시 사용 가능한 상태가 된다.",
            "다시 사용 가능한 상태가 된다（[green]전투 후에도 유지[/green]）。",
        ),
        "deu": (
            "werden für diesen Kampf erneut nutzbar.",
            "werden erneut nutzbar ([green]bleibt nach dem Kampf[/green]).",
        ),
        "spa": (
            "vuelven a estar disponibles solo durante este combate.",
            "vuelven a estar disponibles ([green]persiste tras el combate[/green]).",
        ),
        "rus": (
            "снова становятся доступны на время этого боя.",
            "снова становятся доступны ([green]сохраняется после боя[/green]).",
        ),
        "zhs": (
            "在本次战斗中恢复为可再次使用的状态。",
            "恢复为可再次使用的状态（[green]战斗后仍保留[/green]）。",
        ),
        "zht": (
            "在本次戰鬥中恢復為可再次使用的狀態。",
            "恢復為可再次使用的狀態（[green]戰鬥後仍保留[/green]）。",
        ),
    },
    "HYPNOSISCREATOR-HEARTBEAT_SHARE": {
        "jpn": (
            "味方プレイヤー[blue]1[/blue]人に共有する。",
            "味方プレイヤー[green]すべて[/green]に共有する。",
        ),
        "eng": (
            "with [blue]1[/blue] ally player.",
            "with [green]all[/green] ally players.",
        ),
        "kor": (
            "동료 플레이어 [blue]1[/blue]명과 공유한다.",
            "동료 플레이어 [green]모두[/green]와 공유한다.",
        ),
        "deu": ("mit 1 verbündetem Spieler.", "mit [green]allen[/green] verbündeten Spielern."),
        "spa": ("con 1 jugador aliado.", "con [green]todos[/green] los jugadores aliados."),
        "rus": ("с 1 союзником-игроком.", "со [green]всеми[/green] союзниками-игроками."),
        "zhs": ("共享给[blue]1[/blue]名同伴玩家。", "共享给[green]所有[/green]同伴玩家。"),
        "zht": ("共享給[blue]1[/blue]名同伴玩家。", "共享給[green]所有[/green]同伴玩家。"),
    },
    "HYPNOSISCREATOR-MARSHMALLOW_ANSWER": {
        "jpn": ("ランダムな呪いカード1枚を[gold]手札[/gold]に追加する。", ""),
        "eng": ("Add [blue]1[/blue] random Curse card into your [gold]Hand[/gold]. ", ""),
        "kor": ("무작위 저주 카드 1장을 [gold]손[/gold]에 추가한다.", ""),
        "deu": ("Lege 1 zufällige Fluchkarte in die [gold]Hand[/gold].", ""),
        "spa": ("Añade 1 carta de maldición aleatoria a la [gold]mano[/gold].", ""),
        "rus": ("Добавьте 1 случайную карту проклятия в [gold]руку[/gold].", ""),
        "zhs": ("将一张随机诅咒卡加入[gold]手牌[/gold]。", ""),
        "zht": ("將一張隨機詛咒卡加入[gold]手牌[/gold]。", ""),
    },
    "HYPNOSISCREATOR-RAPPORT": {
        "jpn": (
            "ターン開始時、前のターンに相手を攻撃していなかった場合に[gold]手札[/gold]の[gold]カウント[/gold]がひとつ追加で進む。",
            "ターン開始時、[green][gold]手札[/gold]の[gold]カウント[/gold]がひとつ追加で進む[/green]。",
        ),
        "eng": (
            "At the start of your turn, if you did not attack an opponent last turn, advance [gold]Count[/gold] in your [gold]Hand[/gold] once more.",
            "[green]At the start of your turn, advance [gold]Count[/gold] in your [gold]Hand[/gold] once more.[/green]",
        ),
        "kor": (
            "턴 시작 시, 이전 턴에 상대를 공격하지 않았다면 [gold]손[/gold]에 있는 [gold]카운트[/gold]가 추가로 한 번 더 진행된다.",
            "[green]턴 시작 시, [gold]손[/gold]에 있는 [gold]카운트[/gold]가 한 번 더 진행된다.[/green]",
        ),
        "deu": (
            "Hast du in der letzten Runde den Gegner nicht angegriffen, rückt zu Rundenbeginn ein weiterer [gold]Countdown[/gold] in der [gold]Hand[/gold] vor.",
            "[green]Zu Rundenbeginn rückt ein weiterer [gold]Countdown[/gold] in der [gold]Hand[/gold] vor.[/green]",
        ),
        "spa": (
            "Al inicio del turno, si no atacaste al enemigo el turno anterior, la [gold]Cuenta[/gold] en la [gold]mano[/gold] avanza una vez más.",
            "[green]Al inicio del turno, la [gold]Cuenta[/gold] en la [gold]mano[/gold] avanza una vez más.[/green]",
        ),
        "rus": (
            "В начале хода, если в прошлом ходу вы не атаковали противника, [gold]Счётчик[/gold] в [gold]руке[/gold] продвигается ещё раз.",
            "[green]В начале хода [gold]Счётчик[/gold] в [gold]руке[/gold] продвигается ещё раз.[/green]",
        ),
        "zhs": (
            "回合开始时，若上个回合未攻击对手，则额外多推进一次[gold]计数[/gold]。",
            "[green]回合开始时，额外多推进一次[gold]计数[/gold]。[/green]",
        ),
        "zht": (
            "回合開始時，若上個回合未攻擊對手，則額外多推進一次[gold]計數[/gold]。",
            "[green]回合開始時，額外多推進一次[gold]計數[/gold]。[/green]",
        ),
    },
    "HYPNOSISCREATOR-AS_YOU_WISH": {
        "jpn": (
            "アブノーマル：[gold]筋力[/gold][blue]1[/blue]。SM：[gold]活力[/gold][blue]2[/blue]。DomSub：[blue]1[/blue][gold]ブロック[/gold]。",
            "アブノーマル：[gold]筋力[/gold][blue]1[/blue]と[green][gold]敏捷[/gold][blue]1[/blue][/green]。SM：[gold]活力[/gold][green][blue]4[/blue][/green]。DomSub：[green][blue]2[/blue][/green][gold]ブロック[/gold]。",
        ),
        "eng": (
            "Abnormal: [blue]1[/blue] [gold]Strength[/gold]. SM: [gold]Vigor[/gold] [blue]2[/blue]. DomSub: [blue]1[/blue] [gold]Block[/gold].",
            "Abnormal: [blue]1[/blue] [gold]Strength[/gold] and [green][blue]1[/blue] [gold]Dexterity[/gold][/green]. SM: [green][blue]4[/blue][/green] [gold]Vigor[/gold]. DomSub: [green][blue]2[/blue][/green] [gold]Block[/gold].",
        ),
        "kor": (
            "어브노멀: [gold]힘[/gold][blue]1[/blue]. SM: [gold]활력[/gold][blue]2[/blue]. DomSub: [blue]1[/blue][gold]방어도[/gold].",
            "어브노멀: [gold]힘[/gold][blue]1[/blue]과 [green][gold]민첩[/gold][blue]1[/blue][/green]. SM: [gold]활력[/gold][green][blue]4[/blue][/green]. DomSub: [green][blue]2[/blue][/green][gold]방어도[/gold].",
        ),
        "deu": (
            "Abnormal: [gold]Stärke[/gold] [blue]1[/blue]. SM: [gold]Vitalität[/gold] [blue]2[/blue]. DomSub: [blue]1[/blue] [gold]Block[/gold].",
            "Abnormal: [gold]Stärke[/gold] [blue]1[/blue] und [green][blue]1[/blue] [gold]Geschicklichkeit[/gold][/green]. SM: [green][blue]4[/blue][/green] [gold]Vitalität[/gold]. DomSub: [green][blue]2[/blue][/green] [gold]Block[/gold].",
        ),
        "spa": (
            "Anormal: [gold]Fuerza[/gold] [blue]1[/blue]. SM: [gold]Vigor[/gold] [blue]2[/blue]. DomSub: [blue]1[/blue] de [gold]Bloqueo[/gold].",
            "Anormal: [gold]Fuerza[/gold] [blue]1[/blue] y [green][blue]1[/blue] de [gold]Destreza[/gold][/green]. SM: [green][blue]4[/blue][/green] [gold]Vigor[/gold]. DomSub: [green][blue]2[/blue][/green] de [gold]Bloqueo[/gold].",
        ),
        "rus": (
            "Отклонение: [gold]Сила[/gold] [blue]1[/blue]. SM: [gold]Живучесть[/gold] [blue]2[/blue]. DomSub: [blue]1[/blue] [gold]Блок[/gold].",
            "Отклонение: [gold]Сила[/gold] [blue]1[/blue] и [green][blue]1[/blue] [gold]Ловкость[/gold][/green]. SM: [green][blue]4[/blue][/green] [gold]Живучесть[/gold]. DomSub: [green][blue]2[/blue][/green] [gold]Блок[/gold].",
        ),
        "zhs": (
            "异常：[gold]力量[/gold][blue]1[/blue]。SM：[gold]活力[/gold][blue]2[/blue]。DomSub：[blue]1[/blue][gold]格挡[/gold]。",
            "异常：[gold]力量[/gold][blue]1[/blue]与[green][gold]敏捷[/gold][blue]1[/blue][/green]。SM：[gold]活力[/gold][green][blue]4[/blue][/green]。DomSub：[green][blue]2[/blue][/green][gold]格挡[/gold]。",
        ),
        "zht": (
            "異常：[gold]力量[/gold][blue]1[/blue]。SM：[gold]活力[/gold][blue]2[/blue]。DomSub：[blue]1[/blue][gold]格擋[/gold]。",
            "異常：[gold]力量[/gold][blue]1[/blue]與[green][gold]敏捷[/gold][blue]1[/blue][/green]。SM：[gold]活力[/gold][green][blue]4[/blue][/green]。DomSub：[green][blue]2[/blue][/green][gold]格擋[/gold]。",
        ),
    },
    "HYPNOSISCREATOR-AMBUSH_HYPNOSIS": {
        "jpn": ("相手の数だけカードを引く", "相手の数[green]+1[/green]枚カードを引く"),
        "eng": (
            "Draw cards equal to the number of opponents",
            "Draw cards equal to the number of opponents [green]+ 1[/green]",
        ),
        "kor": ("상대 수만큼 카드를 뽑는다", "상대 수[green]+1[/green]장 카드를 뽑는다"),
        "deu": (
            "Ziehe so viele Karten wie Gegner vorhanden sind",
            "Ziehe so viele Karten wie Gegner vorhanden sind [green]+ 1[/green]",
        ),
        "spa": (
            "Roba tantas cartas como enemigos haya",
            "Roba tantas cartas como enemigos haya [green]+ 1[/green]",
        ),
        "rus": (
            "Возьмите столько карт, сколько противников",
            "Возьмите столько карт, сколько противников [green]+ 1[/green]",
        ),
        "zhs": ("按对手数量抽取卡牌", "按对手数量[green]+1[/green]张抽取卡牌"),
        "zht": ("按對手數量抽取卡牌", "按對手數量[green]+1[/green]張抽取卡牌"),
    },
    "HYPNOSISCREATOR-SUGGESTION_RELEASE": {
        "jpn": ("その数値に応じて", "その数値の[green]2[/green]倍の"),
        "eng": ("equal to the amount removed and", "equal to [green]twice[/green] the amount removed and"),
        "kor": ("그 수치에 따라", "그 수치의 [green]2[/green]배 "),
        "deu": ("entsprechend viele Karten", "[green]doppelt[/green] so viele Karten"),
        "spa": ("la misma cantidad de cartas", "[green]el doble[/green] de cartas"),
        "rus": ("столько же карт, сколько было снято", "[green]вдвое[/green] больше карт"),
        "zhs": ("按其数值", "按其数值[green]2[/green]倍的"),
        "zht": ("按其數值", "按其數值[green]2[/green]倍的"),
    },
}


def main():
    for lang in LANGS:
        path = ROOT / "HypnosisCreator" / "localization" / lang / "cards.json"
        data = json.loads(path.read_text(encoding="utf-8"))
        for entry_id, texts in APPEND.items():
            data[f"{entry_id}.upgradeAppend"] = texts[lang]
        for entry_id, texts in REPLACE.items():
            from_text, to_text = texts[lang]
            data[f"{entry_id}.upgradeReplaceFrom"] = from_text
            data[f"{entry_id}.upgradeReplaceTo"] = to_text
        if lang == "eng":
            data["HYPNOSISCREATOR-SUGGESTION_RELEASE.upgradeEnergyMultiplier"] = "twice"
        path.write_text(
            json.dumps(data, ensure_ascii=False, indent=2) + "\n",
            encoding="utf-8",
        )
        print(f"updated {lang}: {len(APPEND)} append, {len(REPLACE)} replace")


if __name__ == "__main__":
    main()
