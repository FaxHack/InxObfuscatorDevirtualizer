using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using dnlib.DotNet;
using InxObfuscatorDevirtualizer;

namespace ConversionBack
{
    public class Initialize
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", ExactSpelling = true)]
        private static extern IntPtr e(IntPtr intptr, string str);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GetModuleHandle")]
        private static extern IntPtr ab(string str);
        [DllImport("kernel32", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr intptr, string str);

        public static void Init(string resName)
        {
            callingModule = Program.asm.ManifestModule;
            EmbeddedResource resource = (from x in Program.module.Resources where x.Name.Equals(resName) && x.IsPrivate && x.ResourceType == ResourceType.Embedded select x).First() as EmbeddedResource;
            byteArrayResource = exclusiveOR(ReadFully(new MemoryStream(resource.CreateReader().ToArray())));
            OpCode[] array3 = new OpCode[256];
            OpCode[] array4 = new OpCode[256];
            oneByteOpCodes = array3;
            twoByteOpCodes = array4;
            Type typeFromHandle = typeof(OpCode);
            foreach (FieldInfo fieldInfo in typeof(OpCodes).GetFields())
            {
                if (fieldInfo.FieldType == typeFromHandle)
                {
                    OpCode opCode = (OpCode)fieldInfo.GetValue(null);
                    ushort num = (ushort)opCode.Value;
                    if (opCode.Size == 1)
                    {
                        byte b = (byte)num;
                        oneByteOpCodes[(int)b] = opCode;
                    }
                    else
                    {
                        byte b2 = (byte)(num | 65024);
                        twoByteOpCodes[(int)b2] = opCode;
                    }
                }
            }
        }
       
        private static byte[] b(byte[] toEncrypt, int len)
        {
            string key = "HCP";
            byte[] output = toEncrypt;

            for (int i = 0; i < len; i++)
            {
                output[i] = (byte)(toEncrypt[i] ^ key[i % (key.Length)]);
            }
            return output;
        }
         
        public static byte[] DecryptorXor(byte[] data, int datalen, byte[] key, int keylen)
        {
            int N1 = 12;
            int N2 = 14;
            int NS = 258;
            int I = 0;
            for (I = 0; I < keylen; I++)
            {
                NS += NS % (key[I] + 1);
            }

            for (I = 0; I < datalen; I++)
            {
                NS = key[I % keylen] + NS;
                N1 = (NS + 5) * (N1 & 255) + (N1 >> 8);
                N2 = (NS + 7) * (N2 & 255) + (N2 >> 8);
                NS = ((N1 << 8) + N2) & 255;

                data[I] = (byte)((data[I]) ^ NS);
            }
            return b(data, datalen);

        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static byte[] exclusiveOR(byte[] arr1)
        {
            Random rand = new Random(23546654);
            byte[] result = new byte[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
            {
                result[i] = (byte)(arr1[i] ^ rand.Next(0, 250));
            }
            return result;
        }
  
        public static OpCode[] oneByteOpCodes;
        public static OpCode[] twoByteOpCodes;
        public static StackTrace stackTrace;
        public static Module callingModule;
        public static byte[] byteArrayResource;
        public static byte[] byteArrayResource2;
    }
}
