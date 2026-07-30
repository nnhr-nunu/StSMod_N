using System.Reflection;
using System.Text;

class P
{
    static void Main()
    {
        var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
        var t = a.GetType("MegaCrit.Sts2.Core.Commands.CardCmd")!;
        var m = t.GetMethod("Transform", [a.GetType("MegaCrit.Sts2.Core.Models.CardModel")!, a.GetType("MegaCrit.Sts2.Core.Models.CardModel")!, a.GetType("MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle")!])!;
        DumpCalls(m);
    }

    static void DumpCalls(MethodInfo m)
    {
        Console.WriteLine(m.DeclaringType?.Name + "." + m.Name);
        var body = m.GetMethodBody();
        if (body == null) return;
        var il = body.GetILAsByteArray()!;
        var module = m.Module;
        for (int i = 0; i < il.Length; i++)
        {
            byte op = il[i];
            if (op is 0x28 or 0x6F or 0x73)
            {
                try
                {
                    var member = module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    Console.WriteLine((op == 0x73 ? "newobj" : op == 0x28 ? "call" : "callvirt") + " " + member.DeclaringType?.Name + "." + member.Name);
                }
                catch { }
                i += 4;
            }
            else if (op == 0x72)
            {
                try { Console.WriteLine("ldstr \"" + module.ResolveString(BitConverter.ToInt32(il, i + 1)) + "\""); } catch { }
                i += 4;
            }
        }
    }
}
