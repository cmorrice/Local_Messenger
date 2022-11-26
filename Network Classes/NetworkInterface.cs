using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Local_Messenger
{
    public enum MessageType
    {
        disconnect_e,   // when the other disconnects
        connect_e,      // a connect request
        acknowledge_e,  // an acknowledgement
        hash_e,         // a hash request
        upload_e,       // an upload request
        download_e,     // a download request
        delete_e,       // a delete request
        exit_e,         // an exit
    }

    public class NetworkHeader
    {
        public MessageType type;    // uint32_t is typically enum
        public UInt64 payloadSize;   // int64_t is typically ssize_t
        public string fileName;     // character array of up to 256 characters
    }

    public class NetworkInterface
    {
        public static int ERROR = -1;
        public static int NO_ERROR = 0;

        public static int TYPE_SIZE = 4;
        public static int HEADER_SIZE = 267;
        public static int NAME_MAX = 255;
        public static int LONG_SIZE = 8;
        public static int INT_SIZE = 4;


        public NetworkInterface()
        {
            
        }

        public static byte[] readNetworkData(NetworkStream socket, int bytesToRead)
        {
            byte[] thisBuffer = new byte[bytesToRead];

            try
            {
                int bytesRead = 0;

                do
                {
                    bytesRead += socket.Read(thisBuffer, bytesRead, bytesToRead - bytesRead);
                } while (bytesRead != bytesToRead);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("readNetworkData(): failed {0}", e));
                return null;
            }

            return thisBuffer;
        }

        public static int writeNetworkData(NetworkStream socket, byte[] payloadBuffer, int payloadSize)
        {
            try
            {
                socket.Write(payloadBuffer, 0, payloadSize);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("writeNetworkData(): failed {0}", e));
                return ERROR;
            }

            return NO_ERROR;
        }

        private static int readACK(NetworkStream socket)
        {
            MessageType acknowledgeMessage = MessageType.disconnect_e;
            byte[] message = null;

            try
            {
                message = readNetworkData(socket, TYPE_SIZE);
                if (message == null)
                {
                    System.Diagnostics.Debug.WriteLine("readACK(): failed {0}");
                    return ERROR;
                }
                acknowledgeMessage = (MessageType) BitConverter.ToUInt32(message, 0);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("readACK(): failed {0}", e));
                return ERROR;
            }

            if (acknowledgeMessage != MessageType.acknowledge_e)
            {
                System.Diagnostics.Debug.WriteLine("readACK(): ACK is not correct");
                return ERROR;
            }

            return NO_ERROR;
        }

        private static int sendACK(NetworkStream socket)
        {
            MessageType acknowledgeMessage = MessageType.acknowledge_e;
            byte[] payload = BitConverter.GetBytes((UInt32) acknowledgeMessage);

            try
            {
                if (writeNetworkData(socket, payload, TYPE_SIZE) == ERROR)
                {
                    System.Diagnostics.Debug.WriteLine("sendACK(): failed");
                    return ERROR;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("sendACK(): failed {0}", e));
                return ERROR;
            }


            return NO_ERROR;
        }

        public static NetworkHeader readHeader(NetworkStream socket)
        {
            NetworkHeader thisHeader = null;
            byte[] thisBuffer = readNetworkData(socket, HEADER_SIZE);
            if (thisBuffer == null)
            {
                System.Diagnostics.Debug.Write("readHeader(): readNetworkData() failed\n");
                return null;
            }

            thisHeader = bufferToHeader(thisBuffer);
            if (thisHeader == null)
            {
                System.Diagnostics.Debug.Write("readHeader(): bufferToHeader() failed\n");
                return null;
            }

            return thisHeader;
        }

        public static int writeHeader(NetworkStream socket, NetworkHeader inputHeader)
        {
            byte[] thisHeader = headerToBuffer(inputHeader);

            if (writeNetworkData(socket, thisHeader, HEADER_SIZE) == ERROR)
            {
                System.Diagnostics.Debug.Write("writeHeader(): writeNetworkData() failed\n");
                return ERROR;
            }

            return NO_ERROR;
        }

        private static byte[] headerToBuffer(NetworkHeader header)
        {
            byte[] thisBuffer = new byte[HEADER_SIZE];

            try
            {
                BitConverter.GetBytes((UInt32)header.type).CopyTo(thisBuffer, 0);
                BitConverter.GetBytes((UInt64)header.payloadSize).CopyTo(thisBuffer, TYPE_SIZE);
                Encoding.Default.GetBytes(header.fileName).CopyTo(thisBuffer, TYPE_SIZE + LONG_SIZE); // could lead to overflow
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("headerToBuffer(): failed {0}", e));
                return null;
            }

            return thisBuffer;
        }

        private static NetworkHeader bufferToHeader(byte[] thisBuffer)
        {
            NetworkHeader thisHeader = new();

            try
            {
                thisHeader.type = (MessageType) BitConverter.ToUInt32(thisBuffer, 0);
                thisHeader.payloadSize = (UInt64) BitConverter.ToUInt64(thisBuffer, TYPE_SIZE);
                thisHeader.fileName = Encoding.Default.GetString(thisBuffer.TakeLast(NAME_MAX).ToArray()); // could lead to overflow
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("bufferToHeader(): failed {0}", e));
                return null;
            }

            return thisHeader;
        }
    }
}
