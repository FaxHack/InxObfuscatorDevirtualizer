using System.Text;
public class Cryptographer
{
	private byte[] Keys
	{
		get; set;
	}

	public Cryptographer(string password)
	{
		Keys = Encoding.ASCII.GetBytes(password);
	}

	public byte[] Encrypt(byte[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			data[i] ^= Keys[i % Keys.Length];
		}
		return data;
	}

	public byte[] Decrypt(byte[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			data[i] = (byte)(Keys[i % Keys.Length] ^ data[i]);
		}
		return data;
	}

}
