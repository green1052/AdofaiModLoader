using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace AdofaiModLoader.Patcher;

internal static class Program
{
    public static void Main(string[] args)
    {
        string path;

        if (args.Length != 1)
        {
            Console.WriteLine("Provide the location of Assembly-CSharp.dll:");

            path = Console.ReadLine();
        }
        else
            path = args[0];

        ModuleDefMD module = ModuleDefMD.Load(path);

        if (module == null)
        {
            Console.WriteLine($"File {path} not found!");
            return;
        }

        Console.WriteLine($"Loaded {module.Name}");

        Console.WriteLine("Resolving References...");

        module.Context = ModuleDef.CreateModuleContext();

        ((AssemblyResolver)module.Context.AssemblyResolver).AddToCache(module);

        Console.WriteLine("Injecting the Bootstrap Class.");

        ModuleDefMD bootstrap =
            ModuleDefMD.Load(Path.Combine(Directory.GetCurrentDirectory(), "AdofaiModLoader.Bootstrap.dll"));

        Console.WriteLine("Loaded " + bootstrap.Name);

        TypeDef modClass = bootstrap.Types[0];

        foreach (TypeDef type in bootstrap.Types.Where(type => type.Name == "Bootstrap"))
        {
            modClass = type;
            Console.WriteLine($"[Injection] Hooked to: \"{type.Namespace}.{type.Name}\"");
        }

        TypeDef modRefType = modClass;

        bootstrap.Types.Remove(modClass);

        modRefType.DeclaringType = null;

        module.Types.Add(modRefType);

        MethodDef call = FindMethod(modRefType, "Load");

        if (call == null)
        {
            Console.WriteLine($"Failed to get the \"{call.Name}\" method! Maybe you don't have permission?");
            return;
        }

        Console.WriteLine("Injected!");
        Console.WriteLine("Injection completed!");
        Console.WriteLine("Patching code...");

        TypeDef typeDef = FindType(module.Assembly, "ADOStartup");

        MethodDef start = FindMethod(typeDef, "Startup");

        if (start == null)
        {
            start = new MethodDefUser("Startup", MethodSig.CreateInstance(module.CorLibTypes.Void),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            typeDef.Methods.Add(start);
        }

        start.Body.Instructions.Insert(4, OpCodes.Call.ToInstruction(call));

        module.Write(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)), "Assembly-CSharp-Mod.dll"));

        Console.WriteLine("Patching completed successfully!");
    }

    private static MethodDef FindMethod(TypeDef type, string methodName)
    {
        return type?.Methods.FirstOrDefault(method => method.Name == methodName);
    }

    private static TypeDef FindType(AssemblyDef assembly, string path)
    {
        return assembly.Modules.SelectMany(module => module.Types).FirstOrDefault(type => type.FullName == path);
    }
}