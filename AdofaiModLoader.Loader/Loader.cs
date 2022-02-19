using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AdofaiModLoader.API.Interfaces;

namespace AdofaiModLoader.Loader;

public static class Loader
{
    private static readonly string RootPath = Path.Combine(Directory.GetCurrentDirectory(), "Mod");

    private static readonly string PluginPath = Path.Combine(RootPath, "Plugins");

    private static readonly string DependenciesPath = Path.Combine(PluginPath, "dependencies");

    public static void Run()
    {
        LoadDependencies();
        LoadPlugins();
    }

    private static void LoadDependencies()
    {
        foreach (string dependency in Directory
                     .GetFiles(DependenciesPath)
                     .Where(path => path.EndsWith(".dll")))
        {
            try
            {
                LoadAssembly(dependency);
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void LoadPlugins()
    {
        foreach (string pluginPath in Directory.GetFiles(PluginPath)
                     .Where(path => path.EndsWith(".dll")))
        {
            Assembly assembly = LoadAssembly(pluginPath);

            if (assembly == null)
                continue;

            RunPlugin(assembly);
        }
    }

    private static void RunPlugin(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes()
                     .Where(type => type.IsClass && typeof(IPlugin).IsAssignableFrom(type)))
        {
            IPlugin plugin = Activator.CreateInstance(type) as IPlugin;

            try
            {
                plugin?.OnEnabled();
            }
            catch
            {
                try
                {
                    plugin?.OnDisabled();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    private static Assembly LoadAssembly(string path)
    {
        try
        {
            return Assembly.LoadFrom(path);
        }
        catch
        {
            // ignored
        }

        return null;
    }
}