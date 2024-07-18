using System.Numerics;
using System.Text;

namespace CourseRush.Core;

public class RSAUtils
{
    public static BigInteger BiFromHex(string s,int step, bool bigEndian = false)
    {
        byte[] result = new byte[130];
        int sl = s.Length;
        int j = 0;

        for (int i = sl; i > 0; i -= step, ++j)
        {
            int startIndex = Math.Max(i - step, 0);
            int length = Math.Min(i, step);
            result[j] = (byte)HexToDigit(s.Substring(startIndex, length));
        }

        return new BigInteger(result, true, bigEndian);
    }
    
    
    static byte[] ConvertIntArrayToByteArray(short[] intArray)
    {
        // 计算所需的总字节数
        int totalBytes = intArray.Length * sizeof(short);

        // 创建目标 byte 数组
        byte[] byteArray = new byte[totalBytes];

        // 使用 Buffer.BlockCopy 进行转换
        Buffer.BlockCopy(intArray, 0, byteArray, 0, totalBytes);

        return byteArray;
    }

    public static short HexToDigit(string s)
    {
        short result = 0;
        int sl = Math.Min(s.Length, 4);

        for (int i = 0; i < sl; ++i)
        {
            result <<= 4;
            result |= (short)CharToHex(s[i]);
        }

        return result;
    }

    public static int CharToHex(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }
        else if (c >= 'a' && c <= 'f')
        {
            return 10 + c - 'a';
        }
        else if (c >= 'A' && c <= 'F')
        {
            return 10 + c - 'A';
        }
        else
        {
            throw new ArgumentException("Invalid hex character: " + c);
        }
    }
    
    public static string EncryptedString(RSAKey key, string s)
    {
        var a = new int[s.Length];
        var sl = s.Length;
        var i = 0;
        while (i < sl)
        {
            a[i] = s[i];
            i++;
        }

        while (a.Length % key.ChunkSize != 0)
        {
            Array.Resize(ref a, a.Length + 1);
            a[i++] = 0;
        }

        var al = a.Length;
        var result = new StringBuilder();
        for (i = 0; i < al; i += key.ChunkSize)
        {
            var block = new BigInteger(0);
            var j = 0;
            for (var k = i; k < i + key.ChunkSize; ++j)
            {
                block += a[k++];
                block += a[k++] << 8;
            }

            var crypt = BigInteger.ModPow(block, key.E, key.N);
            var text = key.Radix == 16 ? crypt.ToString("X") : crypt.ToString();
            result.Append(text);
        }

        return result.ToString().TrimEnd(); // Remove last space.
    }
    
    public static int BiHighIndex(BigInteger x)
    {
        byte[] bytes = x.ToByteArray();
        int result = bytes.Length - 1;

        while (result > 0 && bytes[result] == 0)
        {
            --result;
        }

        return result;
    }

}


public class RSAKey
{
    public BigInteger N { get; set; }
    public BigInteger E { get; set; }
    public int Radix { get; set; }
    public int ChunkSize { get; set; }

    public RSAKey(string modulus, string exponent)
    {
        E = RSAUtils.BiFromHex(exponent,4);
        N = RSAUtils.BiFromHex(modulus,2);
        ChunkSize = RSAUtils.BiHighIndex(N) - 1;
        Radix = 16;
    }
}