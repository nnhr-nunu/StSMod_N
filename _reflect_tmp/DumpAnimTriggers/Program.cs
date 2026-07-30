using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

class P {
  static void Main() {
    using var fs = File.OpenRead(@"C:\Users\homut\.nuget\packages\alchyr.sts2.baselib\3.3.6\lib\net9.0\BaseLib.dll");
    using var pe = new PEReader(fs);
    var md = pe.GetMetadataReader();
    foreach (var th in md.TypeDefinitions) {
      var t = md.GetTypeDefinition(th);
      if (md.GetString(t.Name) != "TrailPath") continue;
      Console.WriteLine("ns=" + md.GetString(t.Namespace));
      foreach (var mh in t.GetMethods()) {
        var m = md.GetMethodDefinition(mh);
        Console.WriteLine(md.GetString(m.Name) + " RVA=" + m.RelativeVirtualAddress);
        if (m.RelativeVirtualAddress == 0) continue;
        var il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILContent().ToArray();
        for (int i = 0; i < il.Length; ) {
          byte b = il[i];
          if (b == 0x72 && i+4 < il.Length) {
            int token = BitConverter.ToInt32(il, i+1);
            if ((token & unchecked((int)0xFF000000)) == 0x70000000) {
              try { Console.WriteLine("  ldstr " + md.GetUserString(MetadataTokens.UserStringHandle(token & 0x00FFFFFF))); } catch {}
            }
            i += 5; continue;
          }
          if ((b == 0x28 || b == 0x6F || b == 0x7B || b == 0x02 || b == 0x03) && (b < 0x10 || i+4 < il.Length)) {
            if (b == 0x02) { Console.WriteLine("  ldarg.0"); i++; continue; }
            if (b == 0x03) { Console.WriteLine("  ldarg.1"); i++; continue; }
            int token = BitConverter.ToInt32(il, i+1);
            try {
              var h = MetadataTokens.EntityHandle(token);
              if (h.Kind == HandleKind.MemberReference) Console.WriteLine("  call " + md.GetString(md.GetMemberReference((MemberReferenceHandle)h).Name));
              else if (h.Kind == HandleKind.MethodDefinition) Console.WriteLine("  call " + md.GetString(md.GetMethodDefinition((MethodDefinitionHandle)h).Name));
            } catch {}
            i += 5; continue;
          }
          i++;
        }
      }
    }

    // XML docs
  }
}
