using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JumpKingModLoader
{
    /// <summary>
    /// A class whose responsibility is to load in the mod .dll and its references
    /// </summary>
    public class Loader
    {
        /// <summary>
        /// Loads in the Mod .dll and all the references .dlls it can find
        /// </summary>
        public static void Init()
        {
            List<string> loadLog = new List<string>();
            string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Assembly entryAssembly = Assembly.LoadFrom(Path.Combine(basePath, "JumpKingRavensMod.dll"));
            AssemblyName[] referencedAssemblies = entryAssembly.GetReferencedAssemblies();
            for (int i = 0; i < referencedAssemblies.Length; i++)
            {
                string assemblyName = referencedAssemblies[i].Name;
                string assemblyPath = Path.Combine(basePath, $"{assemblyName}.dll");
                loadLog.Add($"Attempting to load {assemblyPath}");
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        // attempt to load it ourselves now
                        var loadedAssembly = Assembly.LoadFrom(assemblyPath);
                        if (loadedAssembly != null)
                        {
                            loadLog.Add($"Successfully loaded assembly at '{assemblyPath}'");
                        }
                        else
                        {
                            loadLog.Add($"Failed to load the assembly at '{assemblyPath}' for an unknown reason");
                        }
                    }
                    catch (Exception e)
                    {
                        // NOTE: The Harmony0.dll currently throws an exception here due to CAS stuff
                        loadLog.Add($"Failed to load the assembly at '{assemblyPath}': {e.ToString()}");
                    }
                }
                else
                {
                    // We trust the clr to find it later itself
                    loadLog.Add($"Assembly '{assemblyPath}' does not exist at the mod location, trusting the CLR to find it later");
                }
            }

            Type type = entryAssembly?.GetType("JumpKingRavensMod.JumpKingRavensModEntry");
            MethodInfo methodInfo = type?.GetMethod("Init");
            methodInfo?.Invoke(null, null);

            try
            {
                File.WriteAllLines("ModLoadLog.txt", loadLog.ToArray());
            }
            catch (Exception e)
            {
                // oh no
            }
        }
    }
}
