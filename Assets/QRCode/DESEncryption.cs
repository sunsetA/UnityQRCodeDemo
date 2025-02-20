using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class DESEncryption
{
    private static readonly string ALGORITHM = "DES";
    private static readonly string TRANSFORMATION = "DES/CBC/PKCS7Padding";
    private static readonly string IV = "configer";

    public static string Encrypt(string plaintext, string key)
    {
        try
        {
            // 创建DES算法实例
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                // 设置加密模式为CBC
                des.Mode = CipherMode.CBC;
                // 设置填充模式为PKCS7
                des.Padding = PaddingMode.PKCS7;

                // 将密钥和初始化向量转换为字节数组
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(IV);

                // 创建加密转换对象
                using (ICryptoTransform encryptor = des.CreateEncryptor(keyBytes, ivBytes))
                {
                    // 将明文转换为字节数组
                    byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                    // 执行加密操作
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
                    // 将加密后的字节数组进行Base64编码
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Encryption error: " + ex.Message);
            return null;
        }
    }


    public static string Decrypt(string ciphertext, string key)
    {
        try
        {
            // 创建DES算法实例
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                // 设置加密模式为CBC
                des.Mode = CipherMode.CBC;
                // 设置填充模式为PKCS7
                des.Padding = PaddingMode.PKCS7;

                // 将密钥和初始化向量转换为字节数组
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(IV);

                // 将Base64编码的密文转换为字节数组
                byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);

                // 创建解密转换对象
                using (ICryptoTransform decryptor = des.CreateDecryptor(keyBytes, ivBytes))
                {
                    // 执行解密操作
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
                    // 将解密后的字节数组转换为字符串
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Decryption error: " + ex.Message);
            return null;
        }
    }

    public static string GenerateRandomChars()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < 4; i++)
        {
            // 随机选择生成数字或字母
            int randomType = UnityEngine.Random.Range(0, 3);
            if (randomType == 0) // 生成数字
            {
                char randomDigit = (char)UnityEngine.Random.Range(48, 58);
                sb.Append(randomDigit);
            }
            else if (randomType == 1) // 生成大写字母
            {
                char randomUpperCaseLetter = (char)UnityEngine.Random.Range(65, 91);
                sb.Append(randomUpperCaseLetter);
            }
            else // 生成小写字母
            {
                char randomLowerCaseLetter = (char)UnityEngine.Random.Range(97, 123);
                sb.Append(randomLowerCaseLetter);
            }
        }
        return sb.ToString();
    }
}