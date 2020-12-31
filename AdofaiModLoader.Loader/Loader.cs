using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AdofaiModLoader.Loader
{
    public static class Loader
    {
        public static void Run()
        {
            LoadDependencies();
            LoadPlugins();
        }

        private static void LoadDependencies()
        {
            foreach (string dependency in Directory
                .GetFiles($"{Directory.GetCurrentDirectory()}\\Mod\\Plugins\\dependencies")
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
            foreach (string pluginPath in Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\Mod\\Plugins")
                .Where(path => path.EndsWith(".dll")))
            {
                Assembly assembly = LoadAssembly(pluginPath);
                RunPlugin(assembly);
            }
        }

        private static void RunPlugin(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes()
                .Where(type => type.IsClass && typeof(Plugin).IsAssignableFrom(type)))
            {
                var plugin = Activator.CreateInstance(type) as Plugin;

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
}