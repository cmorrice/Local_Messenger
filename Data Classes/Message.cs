using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Local_Messenger
{
    class Message
    {
        public Person sender { get; }
        public Person receiver { get; }

        public string content { get; }

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
    }
}
