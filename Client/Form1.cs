using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace TestingGrounds
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
            this.ActiveControl = messageTextBox;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            List<Message> messages = getMessages();

            for (int i = 0; i < messages.Count; i++)
            {
                chatTextBox.AppendText(messages[i].getAuthor() + ": " +
                    DecryptStringFromBytes_Aes(messages[i].getEncryptedText(), messages[i].getAESKey(), messages[i].getIVKey()));
                chatTextBox.AppendText(Environment.NewLine);
            }

            //-------------------------------------------------

            String serverIP = "127.0.0.1";
            int port = 5000;
            client = new TcpClient(serverIP, port);
            stream = client.GetStream();
            Console.WriteLine("Connected to server.");

            //--------------------------------------------
            String name = usernameTextBox.Text;
            byte[] data = System.Text.Encoding.ASCII.GetBytes(name);
            stream.Write(data, 0, data.Length);
            //--------------------------------------------

            readMessageFromServer.RunWorkerAsync();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                String msg = messageTextBox.Text;
                byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
                stream.Write(data, 0, data.Length);
                messageTextBox.Clear();
            }
        }

        //public ArrayList getMessages()
        //{
        //    var dbCon = DBConnection.Instance();
        //    dbCon.DatabaseName = "finalproject";
        //    if (dbCon.IsConnect())
        //    {
        //        //suppose col0 and col1 are defined as VARCHAR in the DB
        //        string query = "SELECT author, message_text FROM messages";
        //        var cmd = new MySqlCommand(query, dbCon.Connection);
        //        var reader = cmd.ExecuteReader();
        //        ArrayList messages = new ArrayList();
        //        while (reader.Read())
        //        {
        //            string author = reader.GetString(0);
        //            string message = reader.GetString(1);
        //            messages.Add(author + ": " + message);
        //        }
        //        dbCon.Close();
        //        return messages;
        //    }
        //    return null;
        //}

        private void readMessageFromServer_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                byte[] buffer = new byte[256];
                String responseString;
                while (true)
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    responseString = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);
                    //Console.WriteLine("Received: " + responseString);
                    readMessageFromServer.ReportProgress(0, responseString);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Cannot read data.");
                stream.Close();
            }
        }

        private void readMessageFromServer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            String response = (String)e.UserState;

            if (response.Contains("$*"))
            {
                String[] responses = response.Split('*');
                switch (responses[0])
                {
                    case "$NEWUSER$":
                        addUserToList(responses[1]);
                        break;

                    case "$REMOVEUSER$":
                        removeUserFromList(responses[1]);
                        break;

                    default:
                        chatTextBox.AppendText(response);
                        chatTextBox.AppendText(Environment.NewLine);
                        break;
                }
            }
            else
            {
                chatTextBox.AppendText(response);
                chatTextBox.AppendText(Environment.NewLine);
            }
        }

        private void addUserToList(String responseString)
        {
            usersTextBox.AppendText(responseString);
            usersTextBox.AppendText(Environment.NewLine);
        }

        private void removeUserFromList(String responseString)
        {
            usersTextBox.Text = usersTextBox.Text.Replace(responseString + "\r\n", "").Trim();
            usersTextBox.AppendText(Environment.NewLine);
        }

        private List<Message> getMessages()
        {
            List<Message> messages = new List<Message>();

            String constring = "datasource=localhost;port=3306; username = root; password =J00bles!; Database=finalproject";
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

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
