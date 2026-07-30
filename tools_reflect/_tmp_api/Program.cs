using System.Reflection;
var sts2Dir = args[0];
foreach (var f in Directory.GetFiles(sts2Dir, "*.dll")) { try { Assembly.LoadFrom(f); } catch {} }
Type[] TypesOf(Assembly asm) { try { return asm.GetTypes(); } catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t=>t!=null).Cast<Type>().ToArray(); } }
Type? Find(string name) => AppDomain.CurrentDomain.GetAssemblies().SelectMany(TypesOf).FirstOrDefault(x => x.Name == name);
var ls = Find("LocString")!;
foreach (var m in ls.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly))
  Console.WriteLine(m);
foreach (var p in ls.GetProperties(BindingFlags.Public|BindingFlags.Instance)) Console.WriteLine("P "+p);
