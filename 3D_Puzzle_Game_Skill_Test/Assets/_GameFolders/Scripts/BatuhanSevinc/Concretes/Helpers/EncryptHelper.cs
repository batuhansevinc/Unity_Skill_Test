using System.Security.Cryptography;
using System.Text;

namespace BatuhanSevinc.Helpers
{
    public static class EncryptHelper
    {
        const string HASH = "f0xle@rn";

        public static string SetEncrypt(string stringData)
        {
            string encryptData = string.Empty;

            byte[] data = UTF8Encoding.UTF8.GetBytes(stringData);

            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(HASH));

                using (TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateEncryptor();
                    byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                    encryptData = System.Convert.ToBase64String(result, 0, result.Length);
                }
            }

            return encryptData;
        }

        public static string GetDecrypt(string stringData)
        {
            string decryptData = string.Empty;

            byte[] data = System.Convert.FromBase64String(stringData);

            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(HASH));

                using (TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateDecryptor();
                    byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                    decryptData = UTF8Encoding.UTF8.GetString(result);
                }
            }

            return decryptData;
        }
    }
}