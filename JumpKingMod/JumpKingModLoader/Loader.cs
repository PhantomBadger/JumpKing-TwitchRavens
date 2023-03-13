using PBJKModBase;
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

            // Load and initialise the mod base
            string pbjkBaseAssemblyPath = Path.Combine(basePath, "PBJKModBase.dll");
            // TODO: Load and call init, move setup logic for statics to that init

            // Gather all mod assemblies
            List<ModAssembly> modAssemblies = GetModAssemblies(basePath, loadLog);

            // Load all the dependencies of each Mod assembly, then invoke it's entry method
            for (int i = 0; i < modAssemblies.Count; i++)
            {
                ModAssembly modAssembly = modAssemblies[i];
                loadLog.Add($"Found Mod Assembly: {modAssembly.ModAttribute.ModName}");
                loadLog.Add($"Attempting to load all referenced assemblies!");

                AssemblyName[] referencedAssemblies = modAssembly.Assembly.GetReferencedAssemblies();
                for (int j = 0; j < referencedAssemblies.Length; j++)
                {
                    string assemblyName = referencedAssemblies[j].Name;
                    string assemblyPath = Path.Combine(basePath, $"{assemblyName}.dll");
                    loadLog.Add($"\tAttempting to load {assemblyPath}");
                    if (File.Exists(assemblyPath))
                    {
                        try
                        {
                            // attempt to load it ourselves now
                            var loadedAssembly = Assembly.LoadFrom(assemblyPath);
                            if (loadedAssembly != null)
                            {
                                loadLog.Add($"\t\tSuccessfully loaded assembly at '{assemblyPath}'");
                            }
                            else
                            {
                                loadLog.Add($"\t\tFailed to load the assembly at '{assemblyPath}' for an unknown reason");
                            }
                        }
                        catch (Exception e)
                        {
                            // NOTE: The Harmony0.dll currently throws an exception here due to CAS stuff
                            loadLog.Add($"\t\tFailed to load the assembly at '{assemblyPath}': {e.ToString()}");
                        }
                    }
                    else
                    {
                        // We trust the clr to find it later itself
                        loadLog.Add($"\t\tAssembly '{assemblyPath}' does not exist at the mod location, trusting the CLR to find it later");
                    }
                }

                // call the entry point for each mod
                MethodInfo methodInfo = modAssembly.EntryType?.GetMethod(modAssembly.ModAttribute.EntryMethod);
                methodInfo?.Invoke(null, null);
            }

            try
            {
                File.WriteAllLines("ModLoadLog.txt", loadLog.ToArray());
            }
            catch (Exception e)
            {
                // oh no
            }
        }

        /// <summary>
        /// Polls all .dll files in the base path and returns a list of all which contain a <see cref="JumpKingModAttribute"/> with the appropriate data aggregated
        /// </summary>
        private static List<ModAssembly> GetModAssemblies(string basePath, List<string> loadLog)
        {          
            string[] candidateAssemblyPaths = Directory.GetFiles(basePath, "*.dll");
            List<ModAssembly> assembliesToLoad = new List<ModAssembly>();
            for (int i = 0; i < candidateAssemblyPaths.Length; i++)
            {
                try
                {
                    Assembly possibleAssembly = Assembly.LoadFrom(candidateAssemblyPaths[i]);

                    // Once loaded try to find an entry
                    Type entryType = possibleAssembly.GetTypes().Where(t => t.IsDefined(typeof(JumpKingModAttribute))).FirstOrDefault();
                    if (entryType == null)
                    {
                        // No valid JK Mod Entry point
                        continue;
                    }

                    JumpKingModAttribute attribute = entryType.GetCustomAttribute<JumpKingModAttribute>();

                    assembliesToLoad.Add(new ModAssembly(possibleAssembly, entryType, attribute));
                }
                catch (Exception e)
                {
                    loadLog.Add($"Error when attempting to load '{candidateAssemblyPaths[i]}': {e.ToString()}");
                }
            }

            return assembliesToLoad;
        }
    }
}
