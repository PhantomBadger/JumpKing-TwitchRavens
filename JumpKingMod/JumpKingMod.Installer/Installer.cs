using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install
{
    /// <summary>
    /// A class which wraps all the required logic for installing a hook to our Mod .dll in a given MonoGame.Framework.dll
    /// </summary>
    public class Installer
    {
        private const string MicrosoftXnaFrameworkGameTypeName = "Microsoft.Xna.Framework.Game";
        private const string InitializeMethodName = "Initialize";

        /// <summary>
        /// Installs a hook into the framework DLL that will call the specified method in the mod DLL
        /// </summary>
        /// <param name="frameworkDLLPath">The full path to the MonoGame.Framework.dll expected</param>
        /// <param name="modDLLPath">The full path to our Mod's .dll</param>
        /// <param name="modEntrySettings">An instance of <see cref="ModEntrySettings"/> which contains info about the hook entry point in our mod</param>
        /// <param name="error">An out parameter that will be populated with error information in the event this fails</param>
        /// <returns>Retuns <see langword="true"/> if successful, if unsuccessful returns <see langword="false"/> and populates <paramref name="error"/> with more info</returns>
        public bool InstallMod(string frameworkDLLPath, string modDLLPath, ModEntrySettings modEntrySettings, out string error)
        {
            error = null;
            if (modEntrySettings == null || string.IsNullOrWhiteSpace(modEntrySettings.EntryClassTypeName) || string.IsNullOrWhiteSpace(modEntrySettings.EntryMethodName))
            {
                error = $"The provided {typeof(ModEntrySettings).Name} instance is null or has invalid data inside";
                return false;
            }

            try
            {
                // Read in the Framework DLL, we'll then get the Initialize function within the Game object and add our init code to the start of it and then finally spit it back out
                using (ModuleDefinition module = ModuleDefinition.ReadModule(frameworkDLLPath, new ReaderParameters() { ReadWrite = true }))
                {
                    // We need to get a reference to the Initialize function
                    TypeDefinition gameType = module?.GetType(MicrosoftXnaFrameworkGameTypeName);
                    MethodDefinition methodDef = gameType?.Methods.FirstOrDefault((MethodDefinition md) => md.Name.Equals(InitializeMethodName));
                    if (methodDef == null)
                    {
                        error = $"Unable to find '{InitializeMethodName}' method in '{MicrosoftXnaFrameworkGameTypeName}' class within the provided '{frameworkDLLPath}' DLL";
                        return false;
                    }

                    // And also a reference to the function we want to call for our mod entry
                    ModuleDefinition entryModule = ModuleDefinition.ReadModule(modDLLPath);
                    TypeDefinition entryType = entryModule?.GetType(modEntrySettings.EntryClassTypeName);
                    MethodDefinition entryMethodDef = entryType?.Methods.FirstOrDefault((MethodDefinition md) => md.Name.Equals(modEntrySettings.EntryMethodName));
                    if (entryMethodDef == null)
                    {
                        error = $"Unable to find the Mod Entry Method '{modEntrySettings.EntryMethodName}' in the '{modEntrySettings.EntryClassTypeName}' class within the provided '{modDLLPath}' DLL";
                        return false;
                    }

                    // We need to tell the Framework DLL to import a reference to our Entry Method, otherwise it won't be able to call it
                    var entryMethodRef = module.ImportReference(entryMethodDef);

                    // Create the IL code we need to run our entry method
                    ILProcessor ilProcessor = methodDef.Body.GetILProcessor();
                    Instruction callInstruction = ilProcessor.Create(OpCodes.Call, entryMethodRef);

                    // Get a reference to the first instruction in the function and add our method call to just before it.
                    Instruction firstInstruction = methodDef.Body.Instructions[0];
                    if (firstInstruction.OpCode.Equals(callInstruction.OpCode))
                    {
                        MethodReference firstOperand = firstInstruction.Operand as MethodReference;
                        MethodReference callOperand = callInstruction.Operand as MethodReference;

                        if (firstOperand.FullName.Equals(callOperand.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            // The mod is already installed!
                            error = "Unable to Install Mod - The mod has already been installed!";
                            return false;
                        }
                    }
                    ilProcessor.InsertBefore(firstInstruction, callInstruction);

                    // Recompute any offsets required for methods in the framework DLL
                    ComputeOffsets(methodDef.Body);

                    // Write out our modified .DLL
                    module.Write();
                }
            }
            catch (Exception e)
            {
                error = $"Encountered Exception during Mod Install: {e.ToString()}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Recomputes the required offsets for the instructions in the provided method
        /// </summary>
        private void ComputeOffsets(Mono.Cecil.Cil.MethodBody body)
        {
            var offset = 0;
            foreach (var instruction in body.Instructions)
            {
                instruction.Offset = offset;
                offset += instruction.GetSize();
            }
        }
    }
}
