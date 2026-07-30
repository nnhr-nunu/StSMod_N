using System.Reflection;

var a = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");

foreach (var type in a.GetTypes().Where(t => t.Name.Contains("VoidForm")))
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        var body = method.GetMethodBody()?.GetILAsByteArray();
        if (body == null) continue;
        for (int i = 0; i < body.Length; i++)
        {
            if (body[i] is not (0x28 or 0x6F)) continue;
            try
            {
                var member = method.Module.ResolveMethod(BitConverter.ToInt32(body, i + 1));
                if (member.Name.Contains("EndTurn") || member.Name.Contains("ReadyToEnd") || member.Name.Contains("EndPlayer"))
                    Console.WriteLine(type.Name + "." + method.Name + " -> " + member.DeclaringType?.Name + "." + member.Name);
            }
            catch { }
        }
    }
}
