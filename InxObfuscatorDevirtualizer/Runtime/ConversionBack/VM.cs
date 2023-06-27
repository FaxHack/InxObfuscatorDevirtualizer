using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Runtime;

namespace ConversionBack
{
	public class VM : Inx
	{
		public static void Execute(object obj, string str)
		{
			if (obj != null)
			{
				return;
			}
			if (str != null)
			{
				return;
			}
            Execute(null, "");
		}

		public static void HandleOpType(int opType, OpCode opcode, ILGenerator ilGenerator, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary, List<LocalBuilder> allLocals)
		{
			switch (opType)
			{
			case 0:
				InlineNoneEmitter(ilGenerator, opcode, binaryReader);
				return;
			case 1:
				InlineMethodEmitter(ilGenerator, opcode, binaryReader);
				return;
			case 2:
				InlineStringEmitter(ilGenerator, opcode, binaryReader);
				return;
			case 3:
				InlineIEmitter(ilGenerator, opcode, binaryReader);
				return;
			case 4:
			case 12:
				InlineVarEmitter(ilGenerator, opcode, binaryReader, allLocals);
				return;
			case 5:
				InlineFieldEmitter(ilGenerator, opcode, binaryReader);
				return;
			case 6:
				InlineTypeEmitter(ilGenerator, opcode, binaryReader);
				return;
			case 7:
				ShortInlineBrTargetEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
				return;
			case 8:
				ShortInlineIEmitter(ilGenerator, opcode, binaryReader);
				return;
			case 9:
				InlineSwitchEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
				return;
			case 10:
				InlineBrTargetEmitter(ilGenerator, opcode, binaryReader, _allLabelsDictionary);
				return;
			case 11:
				InlineTokEmitter(ilGenerator, opcode, binaryReader);
				return;
			case 13:
				ShortInlineREmitter(ilGenerator, opcode, binaryReader);
				return;
			case 14:
				InlineREmitter(ilGenerator, opcode, binaryReader);
				return;
			case 15:
				InlineI8Emitter(ilGenerator, opcode, binaryReader);
				return;
			default:
				throw new Exception("Operand Type Unknown " + opType.ToString());
			}
		}

		private static void InlineNoneEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
		{
			ilGenerator.Emit(opcode);
		}

		private static void InlineMethodEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
		{
			int num = binaryReader.ReadInt32();
			MethodBase methodBase = Initialize.callingModule.ResolveMethod(num);
			if (methodBase is MethodInfo)
			{
				ilGenerator.Emit(opcode, (MethodInfo)methodBase);
				return;
			}
			if (!(methodBase is ConstructorInfo))
			{
				throw new Exception("Check resolvedMethodBase Type");
			}
			ilGenerator.Emit(opcode, (ConstructorInfo)methodBase);
		}
		private static void InlineVarEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, List<LocalBuilder> allLocals)
		{
			int num = binaryReader.ReadInt32();
			if (binaryReader.ReadByte() == 0)
			{
				LocalBuilder localBuilder = allLocals[num];
				ilGenerator.Emit(opcode, localBuilder);
				return;
			}
			ilGenerator.Emit(opcode, num);
		}
		private static void InlineStringEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
		{
			string text = binaryReader.ReadString();
			ilGenerator.Emit(opcode, text);
		}

		private static void InlineIEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
		{
			int num = binaryReader.ReadInt32();
			ilGenerator.Emit(opcode, num);
		}

		private static void InlineFieldEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
		{
			int num = binaryReader.ReadInt32();
			FieldInfo fieldInfo = Initialize.callingModule.ResolveField(num);
			ilGenerator.Emit(opcode, fieldInfo);
		}

		private static void InlineTypeEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
		{
			int num = binaryReader.ReadInt32();
			Type type = Initialize.callingModule.ResolveType(num);
			ilGenerator.Emit(opcode, type);
		}

		private static void ShortInlineBrTargetEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
		{
			int num = binaryReader.ReadInt32();
			Label label = _allLabelsDictionary[num];
			ilGenerator.Emit(opcode, label);
		}

		private static void ShortInlineIEmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
		{
			byte b = binaryReader.ReadByte();
			ilGenerator.Emit(opCode, b);
		}
		private static void ShortInlineREmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
		{
			float num = BitConverter.ToSingle(binaryReader.ReadBytes(4), 0);
			ilGenerator.Emit(opCode, num);
		}

		private static void InlineREmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
		{
			double num = binaryReader.ReadDouble();
			ilGenerator.Emit(opCode, num);
		}

		private static void InlineI8Emitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader)
		{
			long num = binaryReader.ReadInt64();
			ilGenerator.Emit(opCode, num);
		}

		
		private static void InlineSwitchEmitter(ILGenerator ilGenerator, OpCode opCode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
		{
			int num = binaryReader.ReadInt32();
			Label[] array = new Label[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = _allLabelsDictionary[binaryReader.ReadInt32()];
			}
			ilGenerator.Emit(opCode, array);
		}

		private static void InlineBrTargetEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader, Dictionary<int, Label> _allLabelsDictionary)
		{
			int num = binaryReader.ReadInt32();
			Label label = _allLabelsDictionary[num];
			ilGenerator.Emit(opcode, label);
		}

		private static void InlineTokEmitter(ILGenerator ilGenerator, OpCode opcode, BinaryReader binaryReader)
		{
			int num = binaryReader.ReadInt32();
			byte b = binaryReader.ReadByte();
			if (b == 0)
			{
				FieldInfo fieldInfo = Initialize.callingModule.ResolveField(num);
				ilGenerator.Emit(opcode, fieldInfo);
				return;
			}
			if (b == 1)
			{
				Type type = Initialize.callingModule.ResolveType(num);
				ilGenerator.Emit(opcode, type);
				return;
			}
			if (b == 2)
			{
				MethodBase methodBase = Initialize.callingModule.ResolveMethod(num);
				if (methodBase is MethodInfo)
				{
					ilGenerator.Emit(opcode, (MethodInfo)methodBase);
					return;
				}
				if (methodBase is ConstructorInfo)
				{
					ilGenerator.Emit(opcode, (ConstructorInfo)methodBase);
				}
			}
		}

		public static void checkAndSetExceptionHandler(List<FixedExceptionHandlersClass> sorted, int i, ILGenerator ilGenerator)
		{
			foreach (FixedExceptionHandlersClass fixedExceptionHandlersClass in sorted)
			{
				if (fixedExceptionHandlersClass.HandlerType == 1)
				{
					if (fixedExceptionHandlersClass.TryStart == i)
					{
						ilGenerator.BeginExceptionBlock();
					}
					if (fixedExceptionHandlersClass.HandlerEnd == i)
					{
						ilGenerator.EndExceptionBlock();
					}
					if (fixedExceptionHandlersClass.HandlerStart.Contains(i))
					{
						int num = fixedExceptionHandlersClass.HandlerStart.IndexOf(i);
						ilGenerator.BeginCatchBlock(fixedExceptionHandlersClass.CatchType[num]);
					}
				}
				else if (fixedExceptionHandlersClass.HandlerType == 5)
				{
					if (fixedExceptionHandlersClass.TryStart == i)
					{
						ilGenerator.BeginExceptionBlock();
					}
					else if (fixedExceptionHandlersClass.HandlerEnd == i)
					{
						ilGenerator.EndExceptionBlock();
					}
					else if (fixedExceptionHandlersClass.TryEnd == i)
					{
						ilGenerator.BeginFinallyBlock();
					}
				}
			}
		}

		
		public static void processExceptionHandler(BinaryReader bin, int count, MethodBase method, List<ExceptionHandlerClass> _allExceptionHandlerses)
		{
			for (int i = 0; i < count; i++)
			{
				ExceptionHandlerClass exceptionHandlerClass = new ExceptionHandlerClass();
				int num = bin.ReadInt32();
				if (num == -1)
				{
					exceptionHandlerClass.CatchType = null;
				}
				else
				{
					Type type = method.Module.ResolveType(num);
					exceptionHandlerClass.CatchType = type;
				}
				int num2 = bin.ReadInt32();
				exceptionHandlerClass.FilterStart = num2;
				int num3 = bin.ReadInt32();
				exceptionHandlerClass.HandlerEnd = num3;
				int num4 = bin.ReadInt32();
				exceptionHandlerClass.HandlerStart = num4;
				switch (bin.ReadByte())
				{
				case 1:
					exceptionHandlerClass.HandlerType = 1;
					break;
				case 2:
					exceptionHandlerClass.HandlerType = 2;
					break;
				case 3:
					exceptionHandlerClass.HandlerType = 3;
					break;
				case 4:
					exceptionHandlerClass.HandlerType = 4;
					break;
				case 5:
					exceptionHandlerClass.HandlerType = 5;
					break;
				default:
					throw new Exception("Out of Range");
				}
				int num5 = bin.ReadInt32();
				exceptionHandlerClass.TryEnd = num5;
				int num6 = bin.ReadInt32();
				exceptionHandlerClass.TryStart = num6;
				_allExceptionHandlerses.Add(exceptionHandlerClass);
			}
		}

		public static List<FixedExceptionHandlersClass> fixAndSortExceptionHandlers(List<ExceptionHandlerClass> expHandlers)
		{
			List<ExceptionHandlerClass> list = new List<ExceptionHandlerClass>();
			Dictionary<ExceptionHandlerClass, int> dictionary = new Dictionary<ExceptionHandlerClass, int>();
			foreach (ExceptionHandlerClass exceptionHandlerClass in expHandlers)
			{
				if (exceptionHandlerClass.HandlerType == 5)
				{
					dictionary.Add(exceptionHandlerClass, exceptionHandlerClass.TryStart);
				}
				else if (dictionary.ContainsValue(exceptionHandlerClass.TryStart))
				{
					if (exceptionHandlerClass.CatchType != null)
					{
						dictionary.Add(exceptionHandlerClass, exceptionHandlerClass.TryStart);
					}
					else
					{
						list.Add(exceptionHandlerClass);
					}
				}
				else
				{
					dictionary.Add(exceptionHandlerClass, exceptionHandlerClass.TryStart);
				}
			}
			List<FixedExceptionHandlersClass> list2 = new List<FixedExceptionHandlersClass>();
			foreach (KeyValuePair<ExceptionHandlerClass, int> keyValuePair in dictionary)
			{
				if (keyValuePair.Key.HandlerType == 5)
				{
					list2.Add(new FixedExceptionHandlersClass
					{
						TryStart = keyValuePair.Key.TryStart,
						TryEnd = keyValuePair.Key.TryEnd,
						FilterStart = keyValuePair.Key.FilterStart,
						HandlerEnd = keyValuePair.Key.HandlerEnd,
						HandlerType = keyValuePair.Key.HandlerType,
						HandlerStart = { keyValuePair.Key.HandlerStart },
						CatchType = { keyValuePair.Key.CatchType }
					});
				}
				else
				{
					List<ExceptionHandlerClass> list3 = WhereAlternate(list, keyValuePair.Value);
					if (list3.Count == 0)
					{
						list2.Add(new FixedExceptionHandlersClass
						{
							TryStart = keyValuePair.Key.TryStart,
							TryEnd = keyValuePair.Key.TryEnd,
							FilterStart = keyValuePair.Key.FilterStart,
							HandlerEnd = keyValuePair.Key.HandlerEnd,
							HandlerType = keyValuePair.Key.HandlerType,
							HandlerStart = { keyValuePair.Key.HandlerStart },
							CatchType = { keyValuePair.Key.CatchType }
						});
					}
					else
					{
						FixedExceptionHandlersClass fixedExceptionHandlersClass = new FixedExceptionHandlersClass();
						fixedExceptionHandlersClass.TryStart = keyValuePair.Key.TryStart;
						fixedExceptionHandlersClass.TryEnd = keyValuePair.Key.TryEnd;
						fixedExceptionHandlersClass.FilterStart = keyValuePair.Key.FilterStart;
						fixedExceptionHandlersClass.HandlerEnd = list3[list3.Count - 1].HandlerEnd;
						fixedExceptionHandlersClass.HandlerType = keyValuePair.Key.HandlerType;
						fixedExceptionHandlersClass.HandlerStart.Add(keyValuePair.Key.HandlerStart);
						fixedExceptionHandlersClass.CatchType.Add(keyValuePair.Key.CatchType);
						foreach (ExceptionHandlerClass exceptionHandlerClass2 in list3)
						{
							fixedExceptionHandlersClass.HandlerStart.Add(exceptionHandlerClass2.HandlerStart);
							fixedExceptionHandlersClass.CatchType.Add(exceptionHandlerClass2.CatchType);
						}
						list2.Add(fixedExceptionHandlersClass);
					}
				}
			}
			return list2;
		}

		public static List<ExceptionHandlerClass> WhereAlternate(List<ExceptionHandlerClass> exp, int val)
		{
			List<ExceptionHandlerClass> list = new List<ExceptionHandlerClass>();
			foreach (ExceptionHandlerClass exceptionHandlerClass in exp)
			{
				if (exceptionHandlerClass.TryStart == val && exceptionHandlerClass.HandlerType != 5)
				{
					list.Add(exceptionHandlerClass);
				}
			}
			return list;
		}

        public static byte[] byteArrayGrabber(byte[] bytes, int skip, int take)
        {
            byte[] array = new byte[take];
            int num = 0;
            int i = 0;
            while (i < take)
            {
                byte b = bytes[skip + i];
                array[num] = b;
                i++;
                num++;
            }
            return array;
        }

		public static DynamicMethod value;
		public static object locker = new object();
		public static Dictionary<int, DynamicMethod> cache = new Dictionary<int, DynamicMethod>();
		public static EBytes eBytes = new EBytes("Class");
	}
}
