using AutoPocoIO.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AutoPocoIO.DynamicSchema.Util
{
    public static class EncryptDecrypt
    {
        //Encrypt
        public static string EncryptString(string plainText)
        {
            if (AutoPocoConfiguration.IsUsingEncryption)
            {
                byte[] initVectorBytes = Encoding.UTF8.GetBytes(AutoPocoConfiguration.SaltVector);
                byte[] secretKeyBytes = Encoding.UTF8.GetBytes(AutoPocoConfiguration.SecretKey);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

                var hashAlg = SHA256.Create();
                var keyBytes = hashAlg.ComputeHash(secretKeyBytes);

                AesCryptoServiceProvider symmetricKey = new AesCryptoServiceProvider
                {
                    Mode = CipherMode.CBC
                };
                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                return Convert.ToBase64String(cipherTextBytes);
            }
            else
                return plainText;
        }
        //Decrypt
        public static string DecryptString(string cipherText)
        {
            if (AutoPocoConfiguration.IsUsingEncryption)
            {
                byte[] initVectorBytes = Encoding.UTF8.GetBytes(AutoPocoConfiguration.SaltVector);
                byte[] secretKeyBytes = Encoding.UTF8.GetBytes(AutoPocoConfiguration.SecretKey);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

                var hashAlg = SHA256.Create();
                var keyBytes = hashAlg.ComputeHash(secretKeyBytes);
                AesCryptoServiceProvider symmetricKey = new AesCryptoServiceProvider
                {
                    Mode = CipherMode.CBC
                };
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
            else
                return cipherText;
        }
    }
}
