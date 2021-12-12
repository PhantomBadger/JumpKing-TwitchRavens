using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

                    // Get a reference to the assembly load function we want to use
                    ModuleDefinition runtimeModule = ModuleDefinition.ReadModule(typeof(Assembly).Assembly.Location);
                    ModuleDefinition systemModule = ModuleDefinition.ReadModule(typeof(Type).Assembly.Location);
                    TypeDefinition assemblyType = runtimeModule?.GetType(typeof(Assembly).FullName);
                    TypeDefinition typeType = systemModule?.GetType(typeof(Type).FullName);
                    TypeDefinition methodBaseType = runtimeModule?.GetType(typeof(MethodBase).FullName);
                    MethodDefinition loadFromMethodDef = assemblyType?.Methods.FirstOrDefault((MethodDefinition md) => md.Name.Equals("LoadFrom") && md.Parameters.Count == 1);
                    MethodDefinition getTypeMethodDef = assemblyType?.Methods.FirstOrDefault((MethodDefinition md) => md.Name.Equals("GetType") && md.Parameters.Count == 1);
                    MethodDefinition getMethodMethodDef = typeType?.Methods.FirstOrDefault((MethodDefinition md) => md.Name.Equals("GetMethod") && md.Parameters.Count == 1);
                    MethodDefinition invokeMethodDef = methodBaseType?.Methods.FirstOrDefault((MethodDefinition md) => md.Name.Equals("Invoke") && md.Parameters.Count == 2);
                    if (loadFromMethodDef == null || getTypeMethodDef == null)
                    {
                        error = $"Unable to find the Assembly.LoadFrom method within the System.Runtime.dll";
                        return false;
                    }

                    // We need to tell the Framework DLL to import a reference to our Entry Method, otherwise it won't be able to call it
                    var loadFromMethodRef = module.ImportReference(loadFromMethodDef);
                    var getTypeMethodRef = module.ImportReference(getTypeMethodDef);
                    var getMethodMethodRef = module.ImportReference(getMethodMethodDef);
                    var invokeMethodRef = module.ImportReference(invokeMethodDef);

                    // Create the IL code we need to load our DLL at a specified location then call our entry method
                    ILProcessor ilProcessor = methodDef.Body.GetILProcessor();
                    Instruction loadFromArgInstruction = ilProcessor.Create(OpCodes.Ldstr, modDLLPath);
                    Instruction loadFromCallInstruction = ilProcessor.Create(OpCodes.Call, loadFromMethodRef);
                    Instruction getTypeArgInstruction = ilProcessor.Create(OpCodes.Ldstr, modEntrySettings.EntryClassTypeName);
                    Instruction getTypeCallInstruction = ilProcessor.Create(OpCodes.Call, getTypeMethodRef);
                    Instruction getMethodArgInstruction = ilProcessor.Create(OpCodes.Ldstr, modEntrySettings.EntryMethodName);
                    Instruction getMethodCallInstruction = ilProcessor.Create(OpCodes.Call, getMethodMethodRef);
                    Instruction invokeArgInstruction = ilProcessor.Create(OpCodes.Ldnull);
                    Instruction invokeCallInstruction = ilProcessor.Create(OpCodes.Call, invokeMethodRef);
                    Instruction popInstruction = ilProcessor.Create(OpCodes.Pop);
                    //Instruction entryCallInstruction = ilProcessor.Create(OpCodes.Call, entryCallMethod);

                    // Get a reference to the first instruction in the function and add our method call to just before it.
                    Instruction firstInstruction = methodDef.Body.Instructions[0];
                    if (firstInstruction.OpCode.Equals(loadFromArgInstruction.OpCode))
                    {
                        string firstOperand = firstInstruction.Operand as string;
                        string callOperand = loadFromArgInstruction.Operand as string;

                        if (firstOperand.Equals(callOperand, StringComparison.OrdinalIgnoreCase))
                        {
                            // The mod is already installed!
                            //error = "Unable to Install Mod - The mod has already been installed!";
                            //return false;
                            return true;
                        }
                    }
                    ilProcessor.InsertBefore(firstInstruction, loadFromArgInstruction);
                    ilProcessor.InsertBefore(firstInstruction, loadFromCallInstruction);
                    ilProcessor.InsertBefore(firstInstruction, getTypeArgInstruction);
                    ilProcessor.InsertBefore(firstInstruction, getTypeCallInstruction);
                    ilProcessor.InsertBefore(firstInstruction, getMethodArgInstruction);
                    ilProcessor.InsertBefore(firstInstruction, getMethodCallInstruction);
                    ilProcessor.InsertBefore(firstInstruction, invokeArgInstruction);
                    ilProcessor.InsertBefore(firstInstruction, invokeArgInstruction);
                    ilProcessor.InsertBefore(firstInstruction, invokeCallInstruction);
                    ilProcessor.InsertBefore(firstInstruction, popInstruction);

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
