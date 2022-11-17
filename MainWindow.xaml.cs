using System;
using System.Collections.Generic;
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
        public Person me = new Person("Me", "localhost");
        public List<Person> chats = new List<Person>();
        public MainWindow()
        {
            InitializeComponent();
            readSavedMessages();
            refreshWindow();

            Task.Run(() => startServer());
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
            TcpListener server = new TcpListener(IPAddress.Any, 18604);
            // we set our IP address as server's address, and we also set the port: 18604

            server.Start();  // this will start the server

            while (true)   //we wait for a connection
            {
                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it
                this.Dispatcher.Invoke(() =>
                {
                    Server_Out.Text = string.Format("Client Connected: {0}", ((IPEndPoint)client.Client.RemoteEndPoint).Address);
                });
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

                    this.Dispatcher.Invoke(() => { Server_Out.Text = Encoding.Default.GetString(msg); });
                    Console.WriteLine(Encoding.Default.GetString(msg)); //now , we write the message as string
                }
            }
        }

        public async Task startClient()
        {
            await Task.Delay(1000);
            TcpClient client = new TcpClient("127.0.0.1", 18604);
            NetworkStream stream = client.GetStream();
            byte[] hello = Encoding.Default.GetBytes("freaky friday");

            stream.Write(hello, 0, hello.Length);

            while (client.Connected)
            {
                byte[] msg = new byte[1024];
                stream.Read(msg, 0, msg.Length);

                this.Dispatcher.Invoke(() => { Client_Out.Text = Encoding.Default.GetString(msg); });
            }
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

        private void readSavedMessages()
        {
            addTestMessages();
            //writeSavedMessages();
        }

        private void writeSavedMessages()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.ReferenceHandler = ReferenceHandler.Preserve;


            object[] scary = new object[] { me, chats.ToArray() };
            string json = JsonSerializer.Serialize(scary, options);

            File.WriteAllText(string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, "Chat.data"), json);

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
            temp2.addMessage(new Message(me, temp2, new BitmapImage(new Uri("pack://application:,,,/Local Messenger;component/Media/Images/cat.jpg")), DateTime.Parse("11/10/2022 3:33:58 PM")));
            temp2.addMessage(new Message(temp2, me, "solo 2 ala", DateTime.Parse("11/10/2022 3:34:32 PM")));
            temp2.addMessage(new Message(me, temp2, "top me", DateTime.Parse("11/10/2022 3:34:52 PM")));
            temp2.addMessage(new Message(me, temp2, "middle me", DateTime.Parse("11/10/2022 3:35:52 PM")));
            temp2.addMessage(new Message(me, temp2, "bottom me", DateTime.Parse("11/14/2022 8:36:52 PM")));
            temp2.addMessage(new Message(me, temp2, "new bottom me", DateTime.Parse("11/14/2022 10:36:52 PM")));
            temp2.addMessage(temp2, me, "hello :)");
            temp2.addMessage(new Message(temp2, me, new BitmapImage(new Uri("pack://application:,,,/Local Messenger;component/Media/Images/cat.jpg")), DateTime.Now));

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

        private void Search_Box_KeyDown(object sender, KeyEventArgs e)
        {
            List<Message> foundItems = new List<Message>();

            if (e.Key == Key.Enter)
            {
                if (ChatListItem.messageTarget == null)
                {
                    Search_Box.Text = "Select a Chat";
                    return;
                }
                
                foundItems = ChatListItem.messageTarget.searchMessages(Search_Box.Text);
                if (foundItems.Count() > 0)
                {
                    Messages_List.Items.Clear();
                    MessageListItem.AddToListView(Messages_List, foundItems.ToArray(), me);
                }
                else
                {
                    Search_Box.Text = "No results found";
                }
            }
        }

    }
}
