using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class AESEncryption
    {
        //void encrypt()
        //{
        //    string original = "Here is some data to encrypt!";
        //
        //    // Create a new instance of the AesManaged
        //    // class.  This generates a new key and initialization 
        //    // vector (IV).
        //    using (AesManaged myAes = new AesManaged())
        //    {
        //        // Encrypt the string to an array of bytes.
        //        byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);
        //
        //        // Decrypt the bytes to a string.
        //        string roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);
        //
        //        //Display the original data and the decrypted data.
        //        Console.WriteLine("Original:   {0}", original);
        //        Console.WriteLine("Round Trip: {0}", roundtrip);
        //
        //        //Console.WriteLine(System.Text.Encoding.UTF8.GetString(myAes.Key));
        //
        //        sendMessage("RobbyM", encrypted, myAes.Key, myAes.IV);
        //
        //        List<Message> messages = getMessages();
        //
        //        for (int i = 0; i < messages.Count; i++)
        //        {
        //            Console.WriteLine(messages[i].getAuthor() + ": " +
        //                DecryptStringFromBytes_Aes(messages[i].getEncryptedText(), messages[i].getAESKey(), messages[i].getIVKey()));
        //        }
        //        Console.ReadLine();
        //    }
        //}

        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }

        public void sendMessage(String username, byte[] encryptedText, byte[] aesKey, byte[] ivKey)
        {
            String constring = "datasource=localhost;port=3306; username = root; password =J00bles!; Database=finalproject";
            MySqlConnection dbConnection = new MySqlConnection(constring);

            MySqlCommand cmd = new MySqlCommand("INSERT INTO encrypted_messages (author, encryptedText, aesKey, ivKey) VALUES (@Name, @byteData, @aesKey, @ivKey)", dbConnection);

            cmd.Parameters.Add(new MySqlParameter("@Name", username));
            cmd.Parameters.Add(new MySqlParameter("@byteData", encryptedText));
            cmd.Parameters.Add(new MySqlParameter("@aesKey", aesKey));
            cmd.Parameters.Add(new MySqlParameter("@ivKey", ivKey));

            //Console.WriteLine(cmd.CommandText);

            dbConnection.Open();
            cmd.ExecuteNonQuery();
            dbConnection.Close();
        }

        public List<Message> getMessages()
        {
            List<Message> messages = new List<Message>();

            String constring = "datasource=localhost;port=3306; username = root; password =Fr00gles!; Database=finalproject";
            MySqlConnection dbConnection = new MySqlConnection(constring);
            MySqlCommand cmd = new MySqlCommand("SELECT author, encryptedText, aesKey, ivKey FROM encrypted_messages", dbConnection);

            dbConnection.Open();
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                messages.Add(new Message(reader.GetString(0), (byte[])reader.GetValue(1), (byte[])reader.GetValue(2), (byte[])reader.GetValue(3)));
            }
            dbConnection.Close();
            return messages;
        }
    }
}
