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
using System.Diagnostics;

namespace Local_Messenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Person> chats = new List<Person>();
        public Person me = new Person("Me", Dns.GetHostName());
        Server server = new Server();

        public List<Message> sendQueue = new List<Message>();

        public MainWindow()
        {
            InitializeComponent();
            readSavedMessages();
            refreshWindow();
            Server_IP.Text = Dns.GetHostName();
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

            Chat_List.Items.Add(Server_Out);
            Chat_List.Items.Add(Client_Out);

            chats.Sort();
            foreach (Person person in chats)
            {
                Chat_List.Items.Add(new ChatListItem(person));
            }
        }

        public async Task startClient(string serverHostname)
        {
            TcpClient client = null;
            try
            {

                client = new TcpClient(serverHostname, 18604);
            }
            catch (Exception e)
            {
                Debug.WriteLine(serverHostname + " failed");
                Debug.WriteLine(e);
                this.Dispatcher.Invoke(() => { Server_IP.Text = "Failed"; });
                return;
            }

            TcpListener clientServer = null;
            try
            {
                System.Diagnostics.Debug.WriteLine("Client: tcp listener started");
                clientServer = new TcpListener(IPAddress.Any, 18605);

                clientServer.Start();  // this will start the server
                System.Diagnostics.Debug.WriteLine("Client: server started");

                System.Diagnostics.Debug.WriteLine("Client: Waiting for server");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Client: starting server failed {0}", e));
                client.Close();
                return;
            }

            NetworkStream stream = client.GetStream();

            // send the initial handshake
            NetworkHeader handshake = new(MessageType.connect_e, (ulong) me.hostName.Length, me.hostName);
            NetworkInterface.writeHeader(stream, handshake);

            if (NetworkInterface.readACK(stream) == NetworkInterface.ERROR)
            {
                Debug.WriteLine("startClient(): readACK() failed");
                client.Close();
                return;
            }


            // after handshake, set up the receive channel
            try
            {
                TcpClient connection = clientServer.AcceptTcpClient();  //if a connection exists, the server will accept it

                Debug.WriteLine(string.Format("Client: Server Connected: {0}", ((IPEndPoint)connection.Client.RemoteEndPoint).Address));

                Task.Run(() => receiveMessages(connection));
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("Client: failed to accept server: {0}", e));
                client.Close();
                return;
            }




            // send the messages --> first send a header then the message itself
            //foreach (Message message in chats[0].messages)
            //{
            //    sendQueue.Add(message);
            //}

            while (true)
            {
                if (sendQueue.Count == 0)
                {
                    continue;
                }

                Message message = sendQueue.First();
                sendQueue.RemoveAt(0);

                byte[] payload = message.toBuffer();
                NetworkHeader messageHeader = new(MessageType.send_e, (UInt64)payload.Length, "");
                if (NetworkInterface.writeHeader(stream, messageHeader) == NetworkInterface.ERROR)
                {
                    Debug.WriteLine("startClient(): writeHeader failed");
                    break;
                }

                if (NetworkInterface.writeNetworkData(stream, payload, payload.Length) == NetworkInterface.ERROR)
                {
                    Debug.WriteLine("startClient(): writeNetworkData failed");
                    break;
                }
            }

            client.Close();
        }

        private async Task receiveMessages(TcpClient connection)
        {
            string ourHostName = Dns.GetHostName();
            string hostName = Dns.GetHostEntry(((IPEndPoint)connection.Client.RemoteEndPoint).Address).HostName;
            NetworkStream client = connection.GetStream();

            NetworkHeader handshake = NetworkInterface.readHeader(client);
            if (handshake == null)
            {
                System.Diagnostics.Debug.WriteLine("Client: connectToClient(): readHeader failed");
                return;
            }

            if (handshake.type != MessageType.connect_e)
            {
                System.Diagnostics.Debug.WriteLine("Client: header type is not connect_e");

                connection.Close();
                return;
            }

            if (handshake.fileName != hostName)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Client: given hostname is not actual hostname: {0}->{1}", handshake.fileName, hostName));
                connection.Close();
                return;
            }

            if (NetworkInterface.sendACK(client) == NetworkInterface.ERROR)
            {
                Debug.WriteLine("Client: connectToClient(): sendACK() failed");
                connection.Close();
                return;
            }

            while (connection.Connected)  //while the client is connected, we look for incoming messages
            {
                NetworkHeader premessage = NetworkInterface.readHeader(client);
                if (premessage == null)
                {
                    System.Diagnostics.Debug.WriteLine("Client: receiveMessages(): readHeader failed");
                    break;
                }

                // this means that the server is giving us a message
                if (premessage.type == MessageType.send_e)
                {
                    byte[] messageBytes = NetworkInterface.readNetworkData(client, (int)premessage.payloadSize);
                    if (messageBytes == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Client: receiveMessages(): readNetworkData for messageBytes failed");
                        break;
                    }
                    Message message = Message.fromBuffer(messageBytes);
                    System.Diagnostics.Debug.WriteLine("Client: Message Received on Client"); //now , we write the message as string
                    System.Diagnostics.Debug.WriteLine(message.toString()); //now , we write the message as string

                    message.sender.addMessage(message);
                    message.receiver.addMessage(message);
                    refreshWindow();
                }
            }

            connection.Close();
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
            temp1.addMessage(new Message(me, temp1, "I (me) SENT THIS!", DateTime.Now));

            Person temp2 = new Person("ala", "Asus");
            temp2.addMessage(new Message(me, temp2, "dobre miejsce", DateTime.Parse("11/10/2022 3:28:33 PM")));
            temp2.addMessage(new Message(me, temp2, "spelled right pronounced wrong", DateTime.Parse("11/10/2022 3:28:52 PM")));
            temp2.addMessage(new Message(temp2, me, "this is ala's response", DateTime.Parse("11/10/2022 3:29:52 PM")));
            temp2.addMessage(new Message(temp2, me, "middle ala", DateTime.Parse("11/10/2022 3:30:52 PM")));
            temp2.addMessage(new Message(temp2, me, "end ala", DateTime.Parse("11/10/2022 3:31:52 PM")));
            temp2.addMessage(new Message(me, temp2, "solo me", DateTime.Parse("11/10/2022 3:32:52 PM")));
            temp2.addMessage(new Message(temp2, me, "solo ala", DateTime.Parse("11/10/2022 3:33:52 PM")));
            //temp2.addMessage(new Message(me, temp2, new BitmapImage(new Uri("pack://application:,,,/Local Messenger;component/Media/Images/cat.jpg")), DateTime.Parse("11/10/2022 3:33:58 PM")));
            temp2.addMessage(new Message(temp2, me, "solo 2 ala", DateTime.Parse("11/10/2022 3:34:32 PM")));
            temp2.addMessage(new Message(me, temp2, "top me", DateTime.Parse("11/10/2022 3:34:52 PM")));
            temp2.addMessage(new Message(me, temp2, "middle me", DateTime.Parse("11/10/2022 3:35:52 PM")));
            temp2.addMessage(new Message(me, temp2, "bottom me", DateTime.Parse("11/14/2022 8:36:52 PM")));
            temp2.addMessage(new Message(me, temp2, "new bottom me", DateTime.Parse("11/14/2022 10:36:52 PM")));
            temp2.addMessage(new Message(temp2, me, "hello :)", DateTime.Now));
            //temp2.addMessage(new Message(temp2, me, new BitmapImage(new Uri("pack://application:,,,/Local Messenger;component/Media/Images/cat.jpg")), DateTime.Now));
            //Search_Box.Text = temp2.messages[0].toString();


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

        private void Server_Toggle_Click(object sender, RoutedEventArgs e)
        {
            server.window = this;
            Task.Run(() => server.startServer());
        }

        private void Attachment_Button_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void Server_Connect_Click(object sender, RoutedEventArgs e)
        {
            string serverHostname = (string)Server_IP.Text;
            Task.Run(() => startClient(serverHostname));
        }

        private void Start_Chat_Click(object sender, RoutedEventArgs e)
        {
            Person newPerson = new Person(Search_Box.Text, Server_IP.Text);
            chats.Add(newPerson);
            refreshWindow();
        }
    }
}
