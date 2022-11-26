using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
            this.content = content;
            this.sentTimeStamp = timestamp;
            this.type = MessageType.text;
        }

        public Message(Person sender, Person receiver, BitmapImage content, DateTime timestamp)
        {
            this.sender = sender;
            this.receiver = receiver;
            this.content = content;
            this.sentTimeStamp = timestamp;
            this.type = MessageType.image;
        }

        public Message(Person sender, Person receiver, byte[] content, DateTime timestamp)
        {
            this.sender = sender;
            this.receiver = receiver;
            this.content = content;
            this.sentTimeStamp = timestamp;
            this.type = MessageType.file;
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
