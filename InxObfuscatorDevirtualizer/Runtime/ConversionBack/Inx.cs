using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using Runtime;

namespace ConversionBack
{
	public class Inx : Inx2
	{
		public static DynamicMethod Execute(object[] parameters, string Class, MethodBase current)
		{
			string text = Xor.Xoring(Class.ToString());
			string[] array = text.Split(new char[] { '—' });
			int num = IConverter.Convertion(array[1]);
			if (VM.cache.TryGetValue(num, out VM.value))
			{
				return VM.value;
			}
			int num2 = IConverter.Convertion(array[0]);
			int num3 = IConverter.Convertion(array[2]);
			byte[] array2 = VM.byteArrayGrabber(Initialize.byteArrayResource, num2, num3);
			byte[] array3 = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(current.Name));
			byte[] ilasByteArray = current.GetMethodBody().GetILAsByteArray();
            Initialize.DecryptorXor(new Cryptographer("أ\u064b").Encrypt(array2), new Cryptographer("أ\u064b").Encrypt(array2).Length, new Cryptographer("أ\u064b").Encrypt(ilasByteArray), new Cryptographer("أ\u064b").Encrypt(ilasByteArray).Length);
			byte[] array4 = VM.Decrypt(VM.eBytes.Decrypt(array3), array2);
			int num4 = IConverter.Convertion(text.Split(new char[] { '—' })[1]);
			return ConversionBack(current, parameters, num4, new Cryptographer("أ\u064b").Encrypt(array4));
		}

        private static byte[] DecryptBytes(SymmetricAlgorithm alg, byte[] message)
        {
            if (message == null || message.Length == 0)
            {
                return message;
            }
            if (alg == null)
            {
                throw new ArgumentNullException("alg is null");
            }
            byte[] array;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ICryptoTransform cryptoTransform = alg.CreateDecryptor())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(message, 0, message.Length);
                        cryptoStream.FlushFinalBlock();
                        array = memoryStream.ToArray();
                    }
                }
            }
            return array;
        }

        public static byte[] Decrypt(byte[] key, byte[] message)
        {
            byte[] array;
            using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
            {
                rijndaelManaged.Key = key;
                rijndaelManaged.IV = key;
                array = VM.DecryptBytes(rijndaelManaged, message);
            }
            return array;
        }

    }
}
