using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 認知シャッフル経由の虚無化 — 本家 VoidForm の「カードプレイ後にターン強制終了」を抑止するマーカー。
/// </summary>
public class CognitiveVoidBypassPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
