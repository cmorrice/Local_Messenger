﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Local_Messenger
{
    class Person : INotifyPropertyChanged
    {
        private string _name;
        public string name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("name");
            }
        }

        private string _hostName;
        public string hostName
        {
            get => _hostName;
            set
            {
                _hostName = value;
                OnPropertyChanged("hostName");
            }
        }

        public BitmapImage contactPhoto { get; }

        public List<Message> messages { get; }

        public Person()
        {
            contactPhoto = new BitmapImage(new Uri("pack://application:,,,/Local Messenger;component/Media/Images/cat.jpg"));
            messages = new List<Message>();
        }

        public Person(string name, string hostName) : this()
        {
            this.name = name;
            this.hostName = hostName;
        }

        public void sendMessage(Person sender, string content)
        {
            addMessage(sender, this, content);
            // then actually do the sending through the server
        }

        public void addMessage(Person sender, Person receiver, string content)
        {
            Message newMessage = new Message(sender, receiver, content, DateTime.Now);
            messages.Add(newMessage);
        }

        public void addMessage(Message newMessage)
        {
            messages.Add(newMessage);
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
