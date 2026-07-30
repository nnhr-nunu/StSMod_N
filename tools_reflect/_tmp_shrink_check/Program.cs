using System.Reflection;

var path = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";
var asm = Assembly.LoadFrom(path);

var deci = asm.GetType("MegaCrit.Sts2.Core.Models.Monsters.DecimillipedeSegment")!;
var front = asm.GetType("MegaCrit.Sts2.Core.Models.Monsters.DecimillipedeSegmentFront")!;
var shrink = asm.GetType("MegaCrit.Sts2.Core.Models.Powers.ShrinkPower")!;

Console.WriteLine("DecimillipedeSegment.CanChangeScale declared: " +
    deci.GetProperty("CanChangeScale", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.DeclaringType?.Name);

foreach (var t in new[] { front, deci })
{
    var inst = Activator.CreateInstance(t, nonPublic: true);
    try
    {
        var prop = t.GetProperty("CanChangeScale", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null)
            Console.WriteLine($"{t.Name}.CanChangeScale = {prop.GetValue(inst)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{t.Name} instantiate failed: {ex.Message}");
    }
}

// ShrinkPower.AfterApplied source via reading sibling powers - check CreatureCmd scale
var afterApplied = shrink.GetMethod("AfterApplied", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
Console.WriteLine("ShrinkPower.AfterApplied: " + afterApplied?.DeclaringType?.Name);

var monster = asm.GetType("MegaCrit.Sts2.Core.Models.MonsterModel")!;
var mProp = monster.GetProperty("CanChangeScale", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
Console.WriteLine("MonsterModel.CanChangeScale default on MonsterModel: need instance");
