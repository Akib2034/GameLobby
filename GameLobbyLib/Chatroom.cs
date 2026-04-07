using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameLobbyLib
{
    [DataContract]
    [Serializable]
    public class Chatroom
    {
        [DataMember]
        public string RoomName { get; set; }  // name of the room
        [DataMember]
        public List<string> Participants { get; set; } // lists participants/players 
        [DataMember]
        public List<PrivateMessage> PrivateMessages { get; set; } // list of private messages sent 
        [DataMember]
        public List<Message> Messages { get; set; } // list of messages sent publicly 
        [DataMember]
        public List<string> Files { get; set; } // list of files sent in the chatroom
        [DataMember]
        public bool IsPrivate { get; set; } // indicates if the chatroom is private or not

        public Chatroom(string roomName)
        {
            RoomName = roomName; // sets the name of the chatroom
            Participants = new List<string>(); //initializing 
            Messages = new List<Message>(); //initializing 
            Files = new List<string>(); //initializing 
            PrivateMessages = new List<PrivateMessage>(); //initializing 
            IsPrivate = false; 
        }

        public string GetParticipantsAsString()
        {
            string participantsList = string.Join(", ", Participants);
            return participantsList;
        }
    }
}
