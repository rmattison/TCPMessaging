using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingGrounds
{
    class Message
    {
        private String author;
        private byte[] encryptedText;
        private byte[] aesKey;
        private byte[] ivKey;

        public Message(String author, byte[] encryptedText, byte[] aesKey, byte[] ivKey)
        {
            this.author = author;
            this.encryptedText = encryptedText;
            this.aesKey = aesKey;
            this.ivKey = ivKey;
        }

        public String getAuthor() { return author; }
        public byte[] getEncryptedText() { return encryptedText; }
        public byte[] getAESKey() { return aesKey; }
        public byte[] getIVKey() { return ivKey; }
    }
}
