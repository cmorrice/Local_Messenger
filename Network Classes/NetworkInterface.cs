using System;
using System.Collections.Generic;
using System.Linq;
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

    public struct NetworkHeader
    {
        public MessageType type;          // uint32_t is typically enum
        public Int64 payloadSize;              // int64_t is typically ssize_t
        public string fileName;   //
    }

    class NetworkInterface
    {
        public NetworkInterface()
        {
            
        }
    }
}
