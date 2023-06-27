using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ConversionBack
{
	public class EmbeddedDllClass
	{
		public static void ExtractEmbeddedDlls(string dllName, byte[] resourceBytes)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			executingAssembly.GetManifestResourceNames();
			AssemblyName name = executingAssembly.GetName();
			EmbeddedDllClass.tempFolder = string.Format("{0}.{1}.{2}", name.Name, name.ProcessorArchitecture, name.Version);
			string text = Path.Combine(Path.GetTempPath(), EmbeddedDllClass.tempFolder);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string environmentVariable = Environment.GetEnvironmentVariable("PATH");
			string[] array = environmentVariable.Split(new char[] { ';' });
			bool flag = false;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i] == text)
				{
					flag = true;
					if (!flag)
					{
						Environment.SetEnvironmentVariable("PATH", text + ";" + environmentVariable);
					}
					string text2 = Path.Combine(text, dllName);
					bool flag2 = true;
					if (File.Exists(text2))
					{
						byte[] array3 = File.ReadAllBytes(text2);
						if (EmbeddedDllClass.Equality(resourceBytes, array3))
						{
							flag2 = false;
						}
					}
					if (flag2)
					{
						File.WriteAllBytes(text2, resourceBytes);
					}
					return;
				}
			}
		}

		public static bool Equality(byte[] a1, byte[] b1)
		{
			if (a1.Length == b1.Length)
			{
				int i;
				for (i = 0; i < a1.Length; i++)
				{
					if (a1[i] != b1[i])
					{
						break;
					}
				}
				if (i == a1.Length)
				{
					return true;
				}
			}
			return false;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr LoadLibraryEx(string dllToLoad, IntPtr hFile, uint flags);

		public static IntPtr LoadDll(string dllName)
		{
			if (EmbeddedDllClass.tempFolder == string.Empty)
			{
				throw new Exception("Please call ExtractEmbeddedDlls before LoadDll");
			}
			IntPtr intPtr = EmbeddedDllClass.LoadLibraryEx(dllName, IntPtr.Zero, 0U);
			if (intPtr == IntPtr.Zero)
			{
				Exception ex = new Win32Exception();
				throw new DllNotFoundException("Unable to load library: " + dllName + " from " + EmbeddedDllClass.tempFolder, ex);
			}
			return intPtr;
		}
		private static string tempFolder = "";
	}
}
