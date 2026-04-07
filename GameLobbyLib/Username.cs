using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameLobbyLib
{
    [DataContract]
    [Serializable]
    public class Username
    {
        [DataMember]
        public string Name { get; set; } // the username of the user
        public Boolean IsLoggedIn { get; set; } // indicates if the user is logged in or not

        public Username(string name) // constructor to initialize the username object with a chosen name
        {
            Name = name; // sets the username
            IsLoggedIn = true; // set to true, this assumes the user is logged in when created
        }
    }
}
