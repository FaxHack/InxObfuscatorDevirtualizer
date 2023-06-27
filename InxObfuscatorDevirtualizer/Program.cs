using ConversionBack;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InxObfuscatorDevirtualizer
{
    internal class Program
    {
        public static Assembly asm;
        public static ModuleDefMD module;
        static void Main(string[] args)
        {
            Console.Title = "InxObfuscatorDevirter - by 0x29A";
            asm = Assembly.UnsafeLoadFrom(args[0]);
            module = ModuleDefMD.Load(args[0]);
            Initialize.Init(FindInitialiseResourceName());
            foreach (var t in module.Types)
            {
                foreach (var m in t.Methods)
                {
                    if (!m.HasBody) continue;
                    for (int i = 0; i < m.Body.Instructions.Count; i++)
                    {
                        if (m.Body.Instructions[i].OpCode == OpCodes.Call &&
                                    m.Body.Instructions[i].Operand.ToString().Contains("Inx::Execute") && m.Body.Instructions[i - 1].OpCode == OpCodes.Ldstr)
                        {
                            var callmd = m.Body.Instructions[i].Operand as MemberRef;

                            Console.WriteLine($"Devirtualized: {m.FullName}");
                            var stringmd = m.Body.Instructions[i - 1].Operand.ToString();
                            object[] Params = new object[m.Parameters.Count]; int Index = 0;
                            foreach (var Param in m.Parameters) { Params[Index++] = Param.Type.Next; }
                            var methodBase = asm.ManifestModule.ResolveMethod(m.MDToken.ToInt32());
                            var dynamicMethod = Inx.Execute(Params, stringmd, methodBase);
                            var dynamicReader = Activator.CreateInstance(
                                               typeof(System.Reflection.Emit.DynamicMethod).Module.GetTypes()
                                                .FirstOrDefault(tm => tm.Name == "DynamicResolver"),
                                                (System.Reflection.BindingFlags)(-1), null, new object[] { dynamicMethod.GetILGenerator() }, null);
                            var dynamicMethodBodyReader = new DynamicMethodBodyReader(m.Module, dynamicReader);
                            dynamicMethodBodyReader.Read();
                            m.Body = dynamicMethodBodyReader.GetMethod().Body;

                        }
                    }
                }
            }
 
            module.Write("devirtualized.exe", new ModuleWriterOptions(module)
            {
                MetadataOptions = { Flags = MetadataFlags.PreserveAll },
                MetadataLogger = DummyLogger.NoThrowInstance
            });
            Console.ReadKey();
        }
     
        public static string FindInitialiseResourceName()
        {
            foreach (var t in module.Types)
            {
                foreach (var m in t.Methods)
                {
                    if (!m.HasBody) continue;
                    for (int i = 0; i < m.Body.Instructions.Count; i++)
                    {
                        if (m.Body.Instructions[i].OpCode == OpCodes.Call &&
                                      m.Body.Instructions[i].Operand.ToString().Contains("Class::Init") && m.Body.Instructions[i - 1].OpCode == OpCodes.Ldstr)
                        {
                            var stringmd = m.Body.Instructions[i - 1].Operand.ToString();
                            return stringmd;
                        }
                    }
                }
            }
            throw new Exception("Can't find Initer!");
        }

    }
}
