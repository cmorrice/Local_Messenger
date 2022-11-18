using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Local_Messenger
{
    public class Person : INotifyPropertyChanged, IComparable<Person>
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

        private string _draft;
        public string draft
        {
            get => _draft;
            set
            {
                _draft = value;
                OnPropertyChanged("draft");
            }
        }

        [JsonPropertyName("contactPhoto"), JsonConverter(typeof(CustomBitmapConverter))]
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



        // Default comparer for Person type.
        public int CompareTo(Person comparePerson)
        {
            if (comparePerson == null || comparePerson.messages.Count == 0)
            {
                return 1;
            }
            else if (this.messages.Count == 0)
            {
                return -1;
            }
            else
            {
                return comparePerson.messages[^1].sentTimeStamp.CompareTo(this.messages[^1].sentTimeStamp);
            }
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


    public class CustomBitmapConverter : JsonConverter<BitmapImage>
    {
        public override BitmapImage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string image = reader.GetString();

            byte[] byteBuffer = Convert.FromBase64String(image);
            using var stream = new MemoryStream(byteBuffer) { Position = 0 };
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        public override void Write(Utf8JsonWriter writer, BitmapImage value, JsonSerializerOptions options)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(value));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            writer.WriteBase64StringValue(data);
        }

    }
}
