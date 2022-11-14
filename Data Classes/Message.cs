using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Messenger
{
    class Message : INotifyPropertyChanged
    {
        public Person sender { get; }
        public Person receiver { get; }

        private string _content;
        public string content
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
