using System.IO;
using System.Reflection;

namespace AdofaiModLoader.Bootstrap
{
    public sealed class Bootstrap
    {
        private static bool IsLoaded { get; set; }

        public static void Load()
        {
            if (IsLoaded) return;

            try
            {
                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "Mod");

                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);

                Assembly.Load(File.ReadAllBytes(Path.Combine(rootPath, "AdofaiModLoader.Loader.dll")))
                    .GetType("AdofaiModLoader.Loader.Loader")
                    .GetMethod("Run")
                    ?.Invoke(null, null);

                IsLoaded = true;
            }
            catch
            {
                // ignored
            }
        }
    }
}