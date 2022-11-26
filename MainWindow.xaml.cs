using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace Local_Messenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string MessageDirectory = string.Empty;
        private string ContactFileName = "Contacts.txt";
        private string DataFileName = "Chat.txt";  // probably rename to MessageHistoryFileName

        public Person me = new Person("Me", "localhost");
        public List<Person> chats = new List<Person>();

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();



        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            refreshWindow();
        }

        public MainWindow()
        {
            InitializeComponent();
            ReadContactList(ContactFileName);
            ReadSavedMessages(DataFileName);

            Task.Run(() => startServer());

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if true
            WriteContactList(ContactFileName);
            WriteSavedMessages(DataFileName);
            dispatcherTimer.Stop();
#endif
        }


        public void refreshWindow()
        {
            // refresh the chat list
            addChats();

            // refresh the messages
            Messages_List.Items.Clear();
            if (ChatListItem.messageTarget != null)
            {
                MessageListItem.AddToListView(Messages_List, ChatListItem.messageTarget.messages.ToArray(), me);
            }
        }

        public void addChats()
        {
            Chat_List.Items.Clear();
            chats.Sort();
            foreach (Person person in chats)
            {
                Chat_List.Items.Add(new ChatListItem(person));
            }
        }

        public async Task startServer()
        {
            await Task.Run(() =>
            {
                TcpListener server = new TcpListener(IPAddress.Any, 18604);
                // we set our IP address as server's address, and we also set the port: 18604

                server.Start();  // this will start the server

                while (true)   //we wait for a connection
                {
                    TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it
                    this.Dispatcher.Invoke(() =>
                    {
                         Chat_Box.Text = string.Format("Client Connected: {0}", ((IPEndPoint)client.Client.RemoteEndPoint).Address);
                     });

                    NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                    byte[] hello = new byte[100];   //any message must be serialized (converted to byte array)
                    hello = Encoding.Default.GetBytes("hello world");  //conversion string => byte array

                    ns.Write(hello, 0, hello.Length);     //sending the message

                    while (client.Connected)  //while the client is connected, we look for incoming messages
                    {
                        byte[] msg = new byte[1024];     //the messages arrive as byte array
                        ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client

                        this.Dispatcher.Invoke(() => { Chat_Box.Text = Encoding.Default.GetString(msg); });
                        Console.WriteLine(Encoding.Default.GetString(msg)); //now , we write the message as string
                    }
                }
            });
        }

        public void connectServer(string hostname)
        {
            string server = "127.0.0.1";
            TcpClient client = new TcpClient(server, 18604);
            client.Close();
        }

        private void ReadContactList(string FileName)
        {
            if (FileName.Length > 0)
            {
                try
                {
                    StreamReader ContactFile = new StreamReader(FileName);

                    while (!ContactFile.EndOfStream)
                    {
                        string UserName = ContactFile.ReadLine();
                        string HostName = ContactFile.ReadLine();
                        string ImageUri = ContactFile.ReadLine();

                        Person TempPerson = new Person(UserName, HostName, ImageUri);
                        chats.Add(TempPerson);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ReadConectList Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ReadSavedMessages(string FileName)
        {
            char[] Separators = { '~' };

#if true         // read message history and attach to known people
            try
            {
                if (File.Exists(FileName))
                {
                    StreamReader DataFile = new StreamReader(FileName);

                    while (!DataFile.EndOfStream)
                    {
                        string TempSenderUserName;
                        string TempSenderHostName;
                        string TempReceiverUserName;
                        string TempReceiverHostName;
                        DateTime TempTime;
                        string TempMessageText;

                        string[] ElementSizes = DataFile.ReadLine().Split(Separators);

                        int MessageSourceSize = int.Parse(ElementSizes[0]);
                        int MessageSourceHostSize = int.Parse(ElementSizes[1]);
                        int MessageDestinationSize = int.Parse(ElementSizes[2]);
                        int MessageDestinationHostSize = int.Parse(ElementSizes[3]);
                        int MessageTimeStampSize = int.Parse(ElementSizes[4]);
                        int MessageContentSize = int.Parse(ElementSizes[5]);

                        // determine maximize size of any element to create a character buffer that can hold it
                        int MaxSize = Math.Max(Math.Max(MessageSourceSize, MessageSourceHostSize), MessageDestinationSize);
                        int MaxSize2 = Math.Max(Math.Max(MessageDestinationHostSize, MessageTimeStampSize), MessageContentSize);
                        MaxSize = Math.Max(MaxSize, MaxSize2);
                        char[] ReadBuffer = new char[MaxSize + 1];

                        // reads the message source user name into the character buffer called ReadBuffer
                        DataFile.ReadBlock(ReadBuffer, 0, MessageSourceSize);
                        ReadBuffer[MessageSourceSize] = '\0'; // puts null terminator at end of data just read
                        TempSenderUserName = new string(ReadBuffer).Substring(0, MessageSourceSize); // creates a string from ReadBuffer of the message source size

                        // reads the message source host name into the character buffer called ReadBuffer
                        DataFile.ReadBlock(ReadBuffer, 0, MessageSourceHostSize);
                        ReadBuffer[MessageSourceHostSize] = '\0';
                        TempSenderHostName = new string(ReadBuffer).Substring(0, MessageSourceHostSize);

                        DataFile.ReadBlock(ReadBuffer, 0, MessageDestinationSize);
                        ReadBuffer[MessageDestinationSize] = '\0';
                        TempReceiverUserName = new string(ReadBuffer).Substring(0, MessageDestinationSize);

                        DataFile.ReadBlock(ReadBuffer, 0, MessageDestinationHostSize);
                        ReadBuffer[MessageDestinationHostSize] = '\0';
                        TempReceiverHostName = new string(ReadBuffer).Substring(0, MessageDestinationHostSize);

                        DataFile.ReadBlock(ReadBuffer, 0, MessageTimeStampSize);
                        ReadBuffer[MessageTimeStampSize] = '\0';
                        string TempString = new string(ReadBuffer).Substring(0, MessageTimeStampSize);
                        TempTime = DateTime.Parse(TempString);

                        DataFile.ReadBlock(ReadBuffer, 0, MessageContentSize);
                        ReadBuffer[MessageContentSize] = '\0';
                        TempMessageText = new string(ReadBuffer).Substring(0, MessageContentSize);

                        DataFile.ReadLine(); // consume any data to end of line

                        // am I the sender or the receiver?  UserOfInterest is the name of person that is NOT me
                        string UserOfInterest = TempSenderUserName;
                        if (UserOfInterest == me.name)
                        {
                            UserOfInterest = TempReceiverUserName;
                        }


                        // find the person of interest and add a newly created message of "retrieved from file" message components
                        FindPersonByName(UserOfInterest).addMessage(new Message(FindPersonByName(TempSenderUserName),
                                                                                FindPersonByName(TempReceiverUserName),
                                                                                TempMessageText,
                                                                                TempTime));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "readSavedMessages Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
#else
         addTestMessages();
#endif
        }

        private void WriteContactList(string FileName)
        {
            int PersonCount = chats.Count;
            try
            {
                StreamWriter ContactFile = new StreamWriter(FileName);


                for (int counter = 0; counter < PersonCount; counter++)
                {
                    ContactFile.WriteLine(chats[counter].name + "\r\n" + chats[counter].hostName + "\r\n" + chats[counter].imageUri.ToString());
                }

                ContactFile.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WriteContactList Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WriteSavedMessages(string Filename)
        {
            try
            {
                StreamWriter DataFile = new StreamWriter(Filename);
                int NumberPeople = chats.Count;

                for (int PeopleCounter = 0; PeopleCounter < NumberPeople; PeopleCounter++)
                {
                    int TotalMessages = chats[PeopleCounter].messages.Count;
                    for (int MessageCounter = 0; MessageCounter < TotalMessages; MessageCounter++)
                    {
                        WriteMessage(DataFile, chats[PeopleCounter].messages[MessageCounter]);
                    }
                }
                DataFile.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "WriteSavedMessages Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WriteMessage(StreamWriter DataFile, Message MessageToWrite)
        {
            int MessageSourceSize = MessageToWrite.sender.name.Length;
            int MessageSourceHostSize = MessageToWrite.sender.hostName.Length;
            int MessageDestinationSize = MessageToWrite.receiver.name.Length;
            int MessageDestinationHostSize = MessageToWrite.receiver.hostName.Length;

            string TimeStamp = MessageToWrite.sentTimeStamp.ToString();
            int MessageTimeStampSize = TimeStamp.Length;
            int MessageContentSize = MessageToWrite.content.Length;

            if (DataFile != null)
            {
                DataFile.Write(MessageSourceSize.ToString() + "~" +
                               MessageSourceHostSize.ToString() + "~" +
                               MessageDestinationSize.ToString() + "~" +
                               MessageDestinationHostSize.ToString() + "~" +
                               MessageTimeStampSize.ToString() + "~" +
                               MessageContentSize.ToString() + "\r\n");

                DataFile.Write(MessageToWrite.sender.name);
                DataFile.Write(MessageToWrite.sender.hostName);
                DataFile.Write(MessageToWrite.receiver.name);
                DataFile.Write(MessageToWrite.receiver.hostName);
                DataFile.Write(MessageToWrite.sentTimeStamp.ToString());
                DataFile.Write(MessageToWrite.content);
                DataFile.Write("\r\n");

                DataFile.Flush(); // force the data write to filesystem NOW
            }
        }

        private void addTestMessages()
        {
            Person temp1 = new Person("melo", "Spectre");
            temp1.addMessage(new Message(me, temp1, "HELLO MELO", DateTime.Parse("11/09/2022 11:09:52 PM")));
            temp1.addMessage(new Message(temp1, me, "this is melo's response", DateTime.Parse("11/10/2022 6:08:52 PM")));
            temp1.addMessage(new Message(me, temp1, "this is melo's response", DateTime.Parse("11/10/2022 6:09:52 PM")));
            temp1.sendMessage(me, "I (me) SENT THIS!");

            Person temp2 = new Person("ala", "Asus");
            temp2.addMessage(new Message(me, temp2, "dobre miejsce", DateTime.Parse("11/10/2022 3:28:33 PM")));
            temp2.addMessage(new Message(me, temp2, "spelled right pronounced wrong", DateTime.Parse("11/10/2022 3:28:52 PM")));
            temp2.addMessage(new Message(temp2, me, "this is ala's response", DateTime.Parse("11/10/2022 3:29:52 PM")));
            temp2.addMessage(new Message(temp2, me, "middle ala", DateTime.Parse("11/10/2022 3:30:52 PM")));
            temp2.addMessage(new Message(temp2, me, "end ala", DateTime.Parse("11/10/2022 3:31:52 PM")));
            temp2.addMessage(new Message(me, temp2, "solo me", DateTime.Parse("11/10/2022 3:32:52 PM")));
            temp2.addMessage(new Message(temp2, me, "solo ala", DateTime.Parse("11/10/2022 3:33:52 PM")));
            temp2.addMessage(new Message(me, temp2, "top me", DateTime.Parse("11/10/2022 3:34:52 PM")));
            temp2.addMessage(new Message(me, temp2, "middle me", DateTime.Parse("11/10/2022 3:35:52 PM")));
            temp2.addMessage(new Message(me, temp2, "bottom me", DateTime.Parse("11/14/2022 8:36:52 PM")));
            temp2.addMessage(new Message(me, temp2, "new bottom me", DateTime.Parse("11/14/2022 10:36:52 PM")));
            temp2.addMessage(temp2, me, "hello :)");

            Person temp3 = new Person("loseph", "Bing");
            temp3.addMessage(new Message(me, temp3, "jaeni", DateTime.Parse("10/6/2022 10:29:52 PM")));
            temp3.addMessage(new Message(me, temp3, "lebi i josef", DateTime.Parse("10/7/2022 10:29:52 PM")));

            Person temp4 = new Person("k8", "HP");
            temp4.addMessage(new Message(me, temp4, "ye", DateTime.Parse("8/7/2021 10:29:52 PM")));
            temp4.addMessage(new Message(me, temp4, "schlumped", DateTime.Parse("9/7/2021 10:29:52 PM")));

            chats.Add(temp1);
            chats.Add(temp2);
            chats.Add(temp3);
            chats.Add(temp4);

            //messages.Sort();
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ChatListItem.messageTarget == null)
            {
                return;
            }

            // send the message and clear the chat_box
            ChatListItem.messageTarget.sendMessage(me, Chat_Box.Text);
            Chat_Box.Text = string.Empty;

            refreshWindow();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WriteContactList(ContactFileName);
            WriteSavedMessages(DataFileName);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ReadContactList(ContactFileName);
            ReadSavedMessages(DataFileName);
        }

        // Given a name this function iterates through chats to return the person matching the name or null if not found.
        Person FindPersonByName(string Name)
        {
            Person return_value = null;

            if (Name == me.name)  // "me" is not in chats list so need to handle it special!
            {
                return_value = me;
            }
            else
            {

                int NumberPeople = chats.Count;

                for (int PeopleCounter = 0; PeopleCounter < NumberPeople; PeopleCounter++)
                {
                    if (chats[PeopleCounter].name == Name)
                    {
                        return_value = chats[PeopleCounter];
                        break;
                    }
                }
            }
            return (return_value);
        }
    }
}
