using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

class P {
  static void Main() {
    using var fs = File.OpenRead(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
    using var pe = new PEReader(fs);
    var md = pe.GetMetadataReader();

    foreach (var th in md.TypeDefinitions) {
      var t = md.GetTypeDefinition(th);
      var name = md.GetString(t.Name);
      if (!(name.Contains("OfferRoomEndRewards") || (name.Contains("CombatRoom") && name.Contains("Offer")))) continue;
      var declaring = t.GetDeclaringType();
      string owner = !declaring.IsNil ? md.GetString(md.GetTypeDefinition(declaring).Name) : name;
      Console.WriteLine("TYPE " + owner + "/" + name);
      foreach (var mh in t.GetMethods()) {
        var m = md.GetMethodDefinition(mh);
        if (md.GetString(m.Name) != "MoveNext" || m.RelativeVirtualAddress == 0) continue;
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
          if ((b == 0x28 || b == 0x6F || b == 0x7B) && i+4 < il.Length) {
            int token = BitConverter.ToInt32(il, i+1);
            try {
              var h = MetadataTokens.EntityHandle(token);
              if (h.Kind == HandleKind.MemberReference) Console.WriteLine("  call " + md.GetString(md.GetMemberReference((MemberReferenceHandle)h).Name));
              else if (h.Kind == HandleKind.MethodDefinition) Console.WriteLine("  call " + md.GetString(md.GetMethodDefinition((MethodDefinitionHandle)h).Name));
              else if (h.Kind == HandleKind.FieldDefinition) Console.WriteLine("  field " + md.GetString(md.GetFieldDefinition((FieldDefinitionHandle)h).Name));
            } catch {}
            i += 5; continue;
          }
          i++;
        }
      }
    }

    // Also search methods that read ExtraRewards
    foreach (var th in md.TypeDefinitions) {
      var t = md.GetTypeDefinition(th);
      foreach (var mh in t.GetMethods()) {
        var m = md.GetMethodDefinition(mh);
        if (m.RelativeVirtualAddress == 0) continue;
        var il = pe.GetMethodBody(m.RelativeVirtualAddress).GetILContent().ToArray();
        bool hit=false;
        var calls=new System.Collections.Generic.List<string>();
        for (int i=0;i<il.Length-5;) {
          byte b=il[i];
          if ((b==0x28||b==0x6F||b==0x7B)&&i+4<il.Length) {
            int token=BitConverter.ToInt32(il,i+1);
            try {
              var h=MetadataTokens.EntityHandle(token);
              string n=null;
              if (h.Kind==HandleKind.MemberReference) n=md.GetString(md.GetMemberReference((MemberReferenceHandle)h).Name);
              else if (h.Kind==HandleKind.MethodDefinition) n=md.GetString(md.GetMethodDefinition((MethodDefinitionHandle)h).Name);
              else if (h.Kind==HandleKind.FieldDefinition) n="field "+md.GetString(md.GetFieldDefinition((FieldDefinitionHandle)h).Name);
              if (n=="get_ExtraRewards" || n=="field _extraRewards") { hit=true; }
              if (n!=null) calls.Add(n);
            } catch {}
            i+=5; continue;
          }
          i++;
        }
        if (hit) {
          Console.WriteLine("USES ExtraRewards: " + md.GetString(t.Name) + "." + md.GetString(m.Name));
          foreach (var c in calls.Distinct().Where(c => c.Contains("Reward") || c.Contains("Gold") || c.Contains("Extra") || c.Contains("Add")))
            Console.WriteLine("  " + c);
        }
      }
    }
  }
}
