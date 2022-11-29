using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace Local_Messenger
{
    public class Server
    {
        private static Dictionary<string, List<Message>> ServerSendQueues = new();
        private static Dictionary<string, TcpClient> sendConnections = new();
        private static Dictionary<string, TcpClient> receiveConnections = new();
        private static Dictionary<string, bool> newMessage = new(); // a dictionary that will be set to true when a new message is for the hostname

        string ServerHostName;

        public MainWindow window = null;
        public Server()
        {
            ServerHostName = Dns.GetHostName();
        }

        public async Task startServer()
        {
            if (window == null)
            {
                return;
            }


            try
            {
                System.Diagnostics.Debug.WriteLine("Server: tcp listener started");
                TcpListener server = new TcpListener(IPAddress.Any, 18604);


                // we set our IP address as server's address, and we also set the port: 18604

                server.Start();  // this will start the server
                System.Diagnostics.Debug.WriteLine("Server: server started");

                while (true)   //we wait for a connection
                {
                    System.Diagnostics.Debug.WriteLine("Server: Waiting for client");

                    TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it

                    Debug.WriteLine(string.Format("Server: Client Connected: {0}", ((IPEndPoint)client.Client.RemoteEndPoint).Address));


                    // let the client run independently
                    Task.Run(() => { connectToClient(client); });
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Server: startServer(): failed {0}", e));
            }
        }


        public async Task connectToClient(TcpClient connection)
        {
            string hostName = Dns.GetHostEntry(((IPEndPoint)connection.Client.RemoteEndPoint).Address).HostName;
            System.Diagnostics.Debug.WriteLine("Server: connected to " + hostName);


            NetworkStream client = connection.GetStream();

            NetworkHeader handshake = NetworkInterface.readHeader(client);
            if (handshake == null)
            {
                System.Diagnostics.Debug.WriteLine("Server: connectToClient(): readHeader failed");
                return;
            }

            if (handshake.type != MessageType.connect_e)
            {
                System.Diagnostics.Debug.WriteLine("Server: header type is not connect_e");

                connection.Close();
                return;
            }

            if (handshake.fileName != hostName)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Server: given hostname is not actual hostname: {0}->{1}", handshake.fileName, hostName));
                connection.Close();
                return;
            }

            if (NetworkInterface.sendACK(client) == NetworkInterface.ERROR)
            {
                Debug.WriteLine("Server: connectToClient(): sendACK() failed");
                connection.Close();
                return;
            }

            TcpClient sendLane = setupSend(hostName);
            if (sendLane == null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Server: setting up client send failed"));
                connection.Close();
                return;
            }

            Task.Run(() => sendMessages(sendLane, hostName));

            sendConnections[hostName] = sendLane;
            receiveConnections[hostName] = connection;

            while (connection.Connected)  //while the client is connected, we look for incoming messages
            {
                NetworkHeader premessage = NetworkInterface.readHeader(client);
                if (premessage == null)
                {
                    System.Diagnostics.Debug.WriteLine("Server: connectionToClient(): readHeader failed");
                    break;
                }

                // this means that the client wants to send a message
                if (premessage.type == MessageType.send_e)
                {
                    byte[] messageBytes = NetworkInterface.readNetworkData(client, (int) premessage.payloadSize);
                    if (messageBytes == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Server: connectionToClient(): readNetworkData for messageBytes failed");
                        break;
                    }
                    Message message = Message.fromBuffer(messageBytes);
                    System.Diagnostics.Debug.WriteLine("Server: Message Received on Server"); //now , we write the message as string
                    System.Diagnostics.Debug.WriteLine(message.toString()); //now , we write the message as string
                }
            }

            sendLane.Close();
            connection.Close();
            sendConnections.Remove(hostName);
            receiveConnections.Remove(hostName);
        }

        private TcpClient setupSend(string hostName)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(hostName, 18605);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }

            NetworkStream stream = client.GetStream();


            NetworkHeader handshake = new(MessageType.connect_e, (ulong)ServerHostName.Length, ServerHostName);
            NetworkInterface.writeHeader(stream, handshake);

            if (NetworkInterface.readACK(stream) == NetworkInterface.ERROR)
            {
                Debug.WriteLine("Server: setupSend(): readACK() failed");
                return null;
            }

            return client;
        }

        private async Task sendMessages(TcpClient client, string hostName)
        {
            NetworkStream stream = client.GetStream();
            while (true)
            {
                List<Message> sendQueue = ServerSendQueues.GetValueOrDefault(hostName, null);
                if (sendQueue == null || sendQueue.Count == 0)
                {
                    continue;
                }

                Message message = sendQueue.First();
                sendQueue.RemoveAt(0);

                byte[] payload = message.toBuffer();
                NetworkHeader messageHeader = new(MessageType.send_e, (UInt64)payload.Length, "");
                if (NetworkInterface.writeHeader(stream, messageHeader) == NetworkInterface.ERROR)
                {
                    Debug.WriteLine("Server: sendMessages(): writeHeader failed");
                    break;
                }

                if (NetworkInterface.writeNetworkData(stream, payload, payload.Length) == NetworkInterface.ERROR)
                {
                    Debug.WriteLine("Server: sendMessages(): writeNetworkData failed");
                    break;
                }
            }

            client.Close();
        }
    }
}
