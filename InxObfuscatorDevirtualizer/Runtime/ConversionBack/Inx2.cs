using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
namespace ConversionBack
{
	public class Inx2
	{
		public static DynamicMethod ConversionBack(MethodBase callingMethod, object[] parameters, int ID, byte[] bytes)
		{
			MethodBody methodBody = callingMethod.GetMethodBody();
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(new Cryptographer("أ\u064b").Decrypt(bytes)));
			ParameterInfo[] parameters2 = callingMethod.GetParameters();
			List<LocalBuilder> list = new List<LocalBuilder>();
			List<ExceptionHandlerClass> list2 = new List<ExceptionHandlerClass>();
			int num = 0;
			Type[] array;
			if (callingMethod.IsStatic)
			{
				array = new Type[parameters2.Length];
			}
			else
			{
				array = new Type[parameters2.Length + 1];
				array[0] = callingMethod.DeclaringType;
				num = 1;
			}
			for (int i = 0; i < parameters2.Length; i++)
			{
				ParameterInfo parameterInfo = parameters2[i];
				array[num + i] = parameterInfo.ParameterType;
			}
			DynamicMethod dynamicMethod = new DynamicMethod(Xor.Xoring("أ\u064b"), (callingMethod.MemberType == MemberTypes.Constructor) ? null : ((MethodInfo)callingMethod).ReturnParameter.ParameterType, array, Initialize.callingModule, true);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			IList<LocalVariableInfo> localVariables = methodBody.LocalVariables;
            _ = (new Type[localVariables.Count]);
			foreach (LocalVariableInfo localVariableInfo in localVariables)
			{
				list.Add(ilgenerator.DeclareLocal(localVariableInfo.LocalType));
			}
			int num2 = binaryReader.ReadInt32();
			VM.processExceptionHandler(binaryReader, num2, callingMethod, list2);
			List<FixedExceptionHandlersClass> list3 = VM.fixAndSortExceptionHandlers(list2);
			int num3 = binaryReader.ReadInt32();
			Dictionary<int, Label> dictionary = new Dictionary<int, Label>();
			for (int j = 0; j < num3; j++)
			{
				Label label = ilgenerator.DefineLabel();
				dictionary.Add(j, label);
			}
			for (int k = 0; k < num3; k++)
			{
				VM.checkAndSetExceptionHandler(list3, k, ilgenerator);
				short num4 = binaryReader.ReadInt16();
				OpCode opCode;
				if (num4 >= 0 && (int)num4 < Initialize.oneByteOpCodes.Length)
				{
					opCode = Initialize.oneByteOpCodes[(int)num4];
				}
				else
				{
					byte b = (byte)((int)num4 | 65024);
					opCode = Initialize.twoByteOpCodes[(int)b];
				}
				ilgenerator.MarkLabel(dictionary[k]);
				VM.HandleOpType((int)binaryReader.ReadByte(), opCode, ilgenerator, binaryReader, dictionary, list);
			}
			object locker = VM.locker;
			lock (locker)
			{
				if (!VM.cache.ContainsKey(ID))
				{
					VM.cache.Add(ID, dynamicMethod);
				}
			}
			return dynamicMethod;
		}
	}
}
