using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Scrum
{
    public class Encryption
    {
        static string connString = getConnection();
        SqlConnection connect = new SqlConnection(connString);
        public Encryption()
        {
            connString = getConnection();
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select TOP 1 key_vIKey from Keys";
            VIKey = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select TOP 1 key_saltKey from Keys";
            SaltKey = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select TOP 1 key_passwordHash from Keys";
            PasswordHash = cmd.ExecuteScalar().ToString();
            connect.Close();
        }
        protected void setValues()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select TOP 1 key_vIKey from Keys";
            VIKey = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select TOP 1 key_saltKey from Keys";
            SaltKey = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select TOP 1 key_passwordHash from Keys";
            PasswordHash = cmd.ExecuteScalar().ToString();
            connect.Close();
        }
        public static string getConnection()
        {
            Configuration config = new Configuration();
            string conn = config.getConnectionString();
            return conn;
        }
        public static string hash(string clearText)
        {
            //The below works perfectly in hashing to SHA-256 or SHA-512:
            StringBuilder Sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(clearText));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            clearText = Sb.ToString();
            return clearText;
        }
        /*
         The below solution was made publicly by the Visual Studio Forums. It was copied 
         and slightly modified to suit our needs. The code for encryption and decryption 
         was tested in a C# windows application by Saleh Alsyefi and it performed as expected. The PasswordHash, 
         SaltKey, and VIKey are supposed to be secured in a different location other than 
         having them in the same class file. It is suggested to store the keys in the 
         configuration file. For the purpose of our project, I chose to leave in the 
         database. The link to the original code is:
         https://social.msdn.microsoft.com/Forums/vstudio/en-US/d6a2836a-d587-4068-8630-94f4fb2a2aeb/encrypt-and-decrypt-a-string-in-c?forum=csharpgeneral             
         */
        //------------------------ENCRYPTION KEYS------------------------------   
        static string PasswordHash;
        static string SaltKey;
        static string VIKey;
        //------------------------ENCRYPTION METHOD------------------------------
        public static string encrypt(string plainText)
        {
            Encryption encryption = new Encryption();
            plainText = plainText.ToLower();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }
        //------------------------DECRYPTION METHOD------------------------------
        public static string decrypt(string encryptedText)
        {
            Encryption encryption = new Encryption();
            try
            {
                byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
                byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
                var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

                var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
            }
            catch (Exception)
            {
                return encryptedText;
            }
        }
    }
}