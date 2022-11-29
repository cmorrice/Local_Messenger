using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Local_Messenger
{
    public class Message : INotifyPropertyChanged
    {
        public Person sender { get; }
        public Person receiver { get; }

        // dictates whether this message contains text (string), image (BitMapImage), or a file (byte[])
        public enum MessageType
        {
            text, image, file
        }

        public MessageType type { get; }

        private object _content;
        public object content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged("content");
            }
        }

        public DateTime sentTimeStamp { get; }
        DateTime receivedTimeStamp;
        DateTime readTimeStamp;

        public Message(Person sender, Person receiver, string content, DateTime timestamp)
        {
            this.sender = sender;
            this.receiver = receiver;
            this.sentTimeStamp = timestamp;
            this.type = MessageType.text;
            this.content = content;
        }

        public Message(Person sender, Person receiver, BitmapImage content, DateTime timestamp)
        {
            this.sender = sender;
            this.receiver = receiver;
            this.sentTimeStamp = timestamp;
            this.type = MessageType.image;
            this.content = content;
        }

        public Message(Person sender, Person receiver, byte[] content, DateTime timestamp)
        {
            this.sender = sender;
            this.receiver = receiver;
            this.sentTimeStamp = timestamp;
            this.type = MessageType.file;
            this.content = content;
        }

        private static int TYPE_SIZE = NetworkInterface.TYPE_SIZE;
        private static int NAME_MAX = NetworkInterface.NAME_MAX;
        private static int DATE_SIZE = 8;
        private static int INT_SIZE = 4;
        private static int MESSAGE_SIZE = (INT_SIZE * 2) + (NAME_MAX * 2) + DATE_SIZE + TYPE_SIZE;
        public byte[] toBuffer()
        {
            byte[] contentBuffer = null;
            switch ((int)this.type)
            {
                case (int)MessageType.text:
                    contentBuffer = Encoding.Default.GetBytes((string)this.content);
                    break;
                case (int)MessageType.image:
                    contentBuffer = NetworkInterface.imageToBuffer((BitmapImage)this.content);
                    break;
                case (int)MessageType.file:
                    contentBuffer = (byte[])this.content; // TODO FOR WHEN I MAKE A FILE BUT MAYBE THIS IS ACTUALLY GOOD SINCE ITS JUSt bYteS
                    break;
                default: // shouldn't technically be possible because constructor requires one of the above
                    contentBuffer = new byte[0];
                    break;
            }
            byte[] thisBuffer = new byte[MESSAGE_SIZE + contentBuffer.Length];

            byte[] senderName = Encoding.Default.GetBytes(this.sender.hostName);
            byte[] receiverName = Encoding.Default.GetBytes(this.receiver.hostName);
            //System.Diagnostics.Debug.WriteLine(string.Format("sender name: {0} : {1} : {2}", this.sender.hostName, this.sender.hostName.Length, senderName.Length));


            Array.Copy(BitConverter.GetBytes((Int32) senderName.Length), 0, thisBuffer, 0, INT_SIZE);
            Array.Copy(BitConverter.GetBytes((Int32) receiverName.Length), 0, thisBuffer, INT_SIZE, INT_SIZE);

            Array.Copy(senderName, 0, thisBuffer, INT_SIZE * 2, Math.Min(NAME_MAX, senderName.Length));
            Array.Copy(receiverName, 0, thisBuffer, INT_SIZE * 2 + NAME_MAX, Math.Min(NAME_MAX, receiverName.Length));
            BitConverter.GetBytes((Int64) this.sentTimeStamp.ToBinary()).CopyTo(thisBuffer, (INT_SIZE * 2) + (NAME_MAX * 2));
            System.Diagnostics.Debug.WriteLine(string.Format("from {0}", this.sentTimeStamp.ToBinary()));
            System.Diagnostics.Debug.WriteLine(string.Format("from {0}", DateTime.Now.ToBinary()));


            BitConverter.GetBytes((UInt32) this.type).CopyTo(thisBuffer, (INT_SIZE * 2) + (NAME_MAX * 2) + DATE_SIZE);
            contentBuffer.CopyTo(thisBuffer, MESSAGE_SIZE);

            return thisBuffer;
        }

        public static Message fromBuffer(byte[] buffer)
        {
            byte[] jankyTemp = new byte[buffer.Length];
            Buffer.BlockCopy(buffer, 0, jankyTemp, 0, INT_SIZE);
            int senderLength = BitConverter.ToInt32(jankyTemp);

            Buffer.BlockCopy(buffer, INT_SIZE, jankyTemp, 0, INT_SIZE);
            int receiverLength = BitConverter.ToInt32(jankyTemp);

            Buffer.BlockCopy(buffer, INT_SIZE * 2, jankyTemp, 0, NAME_MAX);
            string senderName = Encoding.Default.GetString(jankyTemp.Take(senderLength).ToArray());
            Person sender = Person.findOrCreateHost(senderName);

            Buffer.BlockCopy(buffer, (INT_SIZE * 2) + NAME_MAX, jankyTemp, 0, NAME_MAX);
            string receiverName = Encoding.Default.GetString(jankyTemp.Take(receiverLength).ToArray());
            Person receiver = Person.findOrCreateHost(receiverName);

            byte[] bub = new byte[DATE_SIZE];
            Buffer.BlockCopy(buffer, (INT_SIZE * 2) + NAME_MAX * 2, bub, 0, DATE_SIZE);
            System.Diagnostics.Debug.WriteLine(string.Format("from {0}", BitConverter.ToInt64(bub)));

            DateTime sentTimeStamp = DateTime.FromBinary((Int64)BitConverter.ToInt64(bub));
            

            Buffer.BlockCopy(buffer, (INT_SIZE * 2) + (NAME_MAX * 2) + DATE_SIZE, jankyTemp, 0, TYPE_SIZE);
            MessageType type = (MessageType) BitConverter.ToUInt32(jankyTemp);

            Message thisMessage = null;
            
            byte[] contentBuffer = new byte[buffer.Length - MESSAGE_SIZE];
            Buffer.BlockCopy(buffer, MESSAGE_SIZE, contentBuffer, 0, contentBuffer.Length);

            object content = null;
            switch ((int)type)
            {
                case (int)MessageType.text:
                    content = Encoding.Default.GetString(contentBuffer);
                    thisMessage = new Message(sender, receiver, (string)content, sentTimeStamp);
                    break;
                case (int)MessageType.image:
                    content = NetworkInterface.bufferToImage(contentBuffer);
                    thisMessage = new Message(sender, receiver, (BitmapImage)content, sentTimeStamp);
                    break;
                case (int)MessageType.file:
                    content = contentBuffer;
                    thisMessage = new Message(sender, receiver, (byte[])content, sentTimeStamp);
                    break;
                default:
                    thisMessage = new Message(sender, receiver, string.Empty, sentTimeStamp);
                    break;
            }

            return thisMessage;
        }

        public int sendMessage(NetworkStream socket)
        {

            return NetworkInterface.NO_ERROR;
        }

        public string toString()
        {
            string output = string.Format("Sender Name: {0} : {1}\n", this.sender.name, this.sender.hostName);
            output += string.Format("Receiver Name: {0} : {1}\n", this.receiver.name, this.receiver.hostName);
            output += string.Format("Sent Timestamp: {0}\n", this.sentTimeStamp.ToString());
            output += string.Format("Type: {0}\n", this.type.ToString());
            output += string.Format("Content: {0}\n", (type == MessageType.text) ? (string)this.content : (type == MessageType.image) ? "image" : "file");
            return output;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
