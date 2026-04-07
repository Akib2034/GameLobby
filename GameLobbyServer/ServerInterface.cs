using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.ServiceModel;

using GameLobbyLib;

namespace GameLobbyServer
{
    [ServiceContract]
    public interface ServerInterface
    {
        /*------------------- User Management -------------------*/
        [OperationContract]
        [FaultContract(typeof(ServerException))]
        bool CheckUsername(string username);

        [OperationContract]
        [FaultContract(typeof(ServerException))]
        void CreateUser(string username);

        [OperationContract]
        [FaultContract(typeof(ServerException))]
        void RemoveUser(string username);


        /*------------------- Chatroom Management -------------------*/
        [OperationContract]
        [FaultContract(typeof(ServerException))]
        List<Chatroom> CreateChatroom(string roomName, List<Chatroom> chatrooms);

        [OperationContract]
        [FaultContract(typeof(ServerException))]
        List<Chatroom> JoinChatroom(string roomName, string username, List<Chatroom> chatrooms);

        [OperationContract]
        [FaultContract(typeof(ServerException))]
        List<Chatroom> LeaveChatroom(string roomName, string username, List<Chatroom> chatrooms);

        [OperationContract]
        [FaultContract(typeof(ServerException))]
        List<Chatroom> CreateInitialChatrooms(List<Chatroom> chatrooms);

        [OperationContract]
        [FaultContract(typeof(ServerException))]
        Chatroom GetChatroom(string roomName, List<Chatroom> chatrooms);

        /*------------------- Chatroom Retrieval -------------------*/
        [OperationContract]
        [FaultContract(typeof(ServerException))]
        List<Chatroom> eGetChatroom(string username);

        [OperationContract]
        [FaultContract(typeof(ServerException))]
        List<Chatroom> iGetChatroom();

        /*------------------- Private Chatroom Management -------------------*/
        [OperationContract]
        [FaultContract(typeof(ServerException))]
        List<Chatroom> CreatePrivateChatroom(string sender, string receiver);


        /*------------------- Messaging -------------------*/
        [OperationContract]
        [FaultContract(typeof(ServerException))]
        List<Chatroom> SendMessage(string sender, string roomName, string message, List<Chatroom> chatrooms);

        [OperationContract]
        [FaultContract(typeof(ServerException))]
        void SendPrivateMessage(string sender, string receiver, string message);

        /*------------------- File Handling -------------------*/
        [OperationContract]
        [FaultContract(typeof(ServerException))]
        string UploadFile(string filePath, byte[] fileData, string currentChatroom);
    }
}
