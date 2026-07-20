using System.Reflection;

var a = Assembly.LoadFrom(
    @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var monsterBase = a.GetType("MegaCrit.Sts2.Core.Models.MonsterModel")!;
foreach (var t in a.GetTypes()
             .Where(t => t.IsClass && !t.IsAbstract && monsterBase.IsAssignableFrom(t))
             .OrderBy(t => t.Name))
{
    try
    {
        var inst = Activator.CreateInstance(t)!;
        var id = monsterBase.GetProperty("Id")!.GetValue(inst)!;
        var entry = id.GetType().GetProperty("Entry")!.GetValue(id)!.ToString()!;
        // VisualsPath can NRE outside Godot; derive stem from type name as fallback.
        string vis;
        try
        {
            vis = monsterBase.GetProperty("VisualsPath")!.GetValue(inst)?.ToString() ?? "";
        }
        catch
        {
            vis = "";
        }

        if (string.IsNullOrEmpty(vis))
        {
            // LeafSlimeS -> leaf_slime_s
            var snake = string.Concat(t.Name.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? "_" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
            vis = "res://animations/monsters/" + snake;
        }

        Console.WriteLine(entry + "\t" + t.Name + "\t" + vis);
    }
    catch
    {
        // skip unconstructable
    }
}
