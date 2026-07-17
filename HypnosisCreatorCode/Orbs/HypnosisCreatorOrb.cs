using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Orbs;

/// <summary>性癖オーブの共通ベース。敵スロット表示・植え付け用（パッシブ／励起は未使用）。</summary>
public abstract class HypnosisCreatorOrb : CustomOrbModel
{
    public override bool IncludeInRandomPool => false;

    public override decimal PassiveVal => 0M;
    public override decimal EvokeVal => 0M;

    public abstract Color AccentColor { get; }

    public override Color DarkenedColor => AccentColor.Darkened(0.45f);

    public override string? CustomIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".OrbImagePath();

    public override Node2D? CreateCustomSprite()
    {
        var root = new Node2D();
        var disc = new Polygon2D
        {
            Polygon = BuildCircle(14f, 20),
            Color = AccentColor
        };
        root.AddChild(disc);
        return root;
    }

    public override Task Passive(PlayerChoiceContext choiceContext, Creature? target) =>
        Task.CompletedTask;

    public override Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext) =>
        Task.FromResult<IEnumerable<Creature>>([]);

    private static Vector2[] BuildCircle(float radius, int segments)
    {
        var points = new Vector2[segments];
        for (var i = 0; i < segments; i++)
        {
            var angle = i * Mathf.Tau / segments;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        return points;
    }
}
