using System.Security.Cryptography;
using System.Text;

namespace Okta.Aws.Cli.Encryption;

public class AesOperation
{
    public static string EncryptString(string plainText)
    {
        var key = GetKeyByMachine();
        
        byte[] iv = new byte[16];
        byte[] array;

        var rawKey = Encoding.UTF8.GetBytes(key);
        using (Aes aes = Aes.Create())
        {
            aes.Key = rawKey;
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream =
                       new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }

                    array = memoryStream.ToArray();
                }
            }
        }

        return Convert.ToBase64String(array);
    }

    public static string DecryptString(string cipherText)
    {
        var key = GetKeyByMachine();
        
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream =
                       new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }

    public static string GetKeyByMachine()
    {
        
        var user = Environment.UserName;
        var processorCount = Environment.ProcessorCount;
        var machineName = Environment.MachineName;

        var key = $"{user}_{processorCount}_{machineName}";
        if (key.Length < 32)
        {
            key = key.PadRight(32, '0');
            return key;
        }

        return key.Take(32).ToString()!;
    }
}