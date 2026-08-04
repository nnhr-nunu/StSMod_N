using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>キャラ選択画面のロビー状態（ソロ／マルチ）を判定する。</summary>
internal static class CharacterSelectScreenUtil
{
    private static readonly System.Reflection.FieldInfo? LobbyField =
        AccessTools.Field(typeof(NCharacterSelectScreen), "_lobby");

    public static bool IsMultiplayerLobbyActive()
    {
        var screen = FindCharacterSelectScreen();
        if (screen == null)
            return false;

        if (LobbyField?.GetValue(screen) is not StartRunLobby lobby)
            return false;

        return lobby.NetService.Type.IsMultiplayer();
    }

    private static NCharacterSelectScreen? FindCharacterSelectScreen()
    {
        var root = Godot.Engine.GetMainLoop() as Godot.SceneTree;
        if (root == null)
            return null;

        var stack = new System.Collections.Generic.Stack<Godot.Node>();
        stack.Push(root.Root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node is NCharacterSelectScreen screen)
                return screen;

            foreach (var child in node.GetChildren())
                stack.Push(child);
        }

        return null;
    }
}
