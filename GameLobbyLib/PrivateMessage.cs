using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GameLobbyLib
{
    [DataContract]
    [Serializable]
    public class PrivateMessage
    {
        [DataMember]
        public string Sender { get; set; } // the username of the message sender
        [DataMember]
        public string Receiver { get; set; } // the username of the message receiver
        [DataMember]
        public string Content { get; set; } // content of the private message
    }
}
