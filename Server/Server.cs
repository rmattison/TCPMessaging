using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        const string SERVER_IP = "127.0.0.1";
        const int PORT_NO = 5000;
        public static int i;
        

        public static List<ClientHandler> clientList = new List<ClientHandler>();

        static void Main(string[] args)
        {
            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener server = new TcpListener(localAdd, PORT_NO);
            TcpClient client;

            server.Start();
            Console.WriteLine("Server Started.");

            i = 0;

            while (true)
            {
                Console.WriteLine("Listening...");
                client = server.AcceptTcpClient();
                Console.Write("Client connected: " + client.Client.RemoteEndPoint);

                //---get the incoming data through a network stream---
                NetworkStream stream = client.GetStream();

                ClientHandler newClient = new ClientHandler(i, client, stream);
                clientList.Add(newClient);
                i++;
            }
        }
    }

    class ClientHandler
    {
        String username;
        int id;
        TcpClient client;
        NetworkStream stream;
        Boolean isLoggedIn = true;

        public ClientHandler(int id, TcpClient client, NetworkStream stream)
        {
            this.id = id;
            this.client = client;
            this.stream = stream;
            this.isLoggedIn = true;

            Thread clientThread = new Thread(Chat);
            clientThread.Start();
        }

        private void Chat()
        {
            try
            {
                //---read incoming stream---
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);

                //---convert the data received into a string---
                this.username = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                newClientAdded(username);

                byte[] data;
                foreach (ClientHandler client in Server.clientList)
                {
                    if (client.isLoggedIn == true)
                    {
                        data = System.Text.Encoding.ASCII.GetBytes("$NEWUSER$*" + client.username);
                        this.stream.Write(data, 0, data.Length);
                        stream.Flush();
                    }
                }

                while (true)
                {
                    //---read incoming stream---
                    buffer = new byte[client.ReceiveBufferSize];
                    bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);

                    //---convert the data received into a string---
                    String dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    data = System.Text.Encoding.ASCII.GetBytes(username + ": " + dataReceived);

                    if (String.IsNullOrWhiteSpace(dataReceived))
                    {
                        throw new Exception();
                    }

                    //---display received on server screen---
                    Console.WriteLine(username + ": " + dataReceived);

                    encryptString(dataReceived, username);

                    foreach (ClientHandler client in Server.clientList)
                    {
                        if (client.isLoggedIn==true)
                        {
                            client.stream.Write(data, 0, data.Length);
                        }
                    }                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Closing client and stream.");
                this.stream.Close();
                this.client.Close();
                this.isLoggedIn = false;

                byte[] data = System.Text.Encoding.ASCII.GetBytes("$REMOVEUSER$*" + username);
                foreach (ClientHandler client in Server.clientList)
                {
                    if (client.isLoggedIn == true)
                    {
                        client.stream.Write(data, 0, data.Length);
                    }
                }
            }
        }

        private void newClientAdded(String username)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("$NEWUSER$*" + username);
            foreach (ClientHandler client in Server.clientList)
            {
                if (client.isLoggedIn == true && client!=this)
                {
                    client.stream.Write(data, 0, data.Length);
                }
            }
        }

        void encryptString(String orig, String user)
        {
            AESEncryption MyAES = new AESEncryption();

            string original = orig;

            // Create a new instance of the AesManaged
            // class.  This generates a new key and initialization 
            // vector (IV).
            using (AesManaged myAes = new AesManaged())
            {
                // Encrypt the string to an array of bytes.
                byte[] encrypted = MyAES.EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);
                Console.WriteLine(Encoding.Default.GetString(encrypted));
                Console.WriteLine();

                //Display the original data and the decrypted data.
                //Console.WriteLine("Original:   {0}", original);

                //Console.WriteLine(System.Text.Encoding.UTF8.GetString(myAes.Key));

                MyAES.sendMessage(user, encrypted, myAes.Key, myAes.IV);

                //List<Message> messages = MyAES.getMessages();
                //
                //for (int i = 0; i < messages.Count; i++)
                //{
                //    Console.WriteLine(messages[i].getAuthor() + ": " +
                //        MyAES.DecryptStringFromBytes_Aes(messages[i].getEncryptedText(), messages[i].getAESKey(), messages[i].getIVKey()));
                //}
                //Console.ReadLine();
            }
        }

        void decryptBytes(byte[] encryptedIn, String user)
        {
            AESEncryption MyAES = new AESEncryption();


            // Create a new instance of the AesManaged
            // class.  This generates a new key and initialization 
            // vector (IV).
            using (AesManaged myAes = new AesManaged())
            {
                // Encrypt the string to an array of bytes.
                byte[] encrypted = encryptedIn;

                // Decrypt the bytes to a string.
                //string decrypted = MyAES.DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);

                //Display the original data and the decrypted data.
                //Console.WriteLine("Decrypted: {0}", decrypted);

                //Console.WriteLine(System.Text.Encoding.UTF8.GetString(myAes.Key));

                List<Message> messages = MyAES.getMessages();

                //Console.WriteLine(messages[i].getAuthor() + ": " +
                //        MyAES.DecryptStringFromBytes_Aes(messages[i].getEncryptedText(), 
                //        messages[i].getAESKey(), messages[i].getIVKey()));
                //Console.ReadLine();
            }
        }
    }
}
