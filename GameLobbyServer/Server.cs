using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Diagnostics;
using System.IO;

using GameLobbyLib;

namespace GameLobbyServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class Server : ServerInterface
    {
        public static List<Chatroom> ChatroomsList { get; private set; } = new List<Chatroom>();
        public static List<Username> UsernamesList { get; private set; } = new List<Username>();

        /*------------------- User Management -------------------*/

        public bool CheckUsername(string username)
        {
            if (UsernamesList.Any(user => user.Name == username))
            {
                Console.WriteLine("User exists: " + username);
                return true;
            }
            return false;
        }

        public void CreateUser(string username)
        {
            if (!CheckUsername(username))
            {
                Console.WriteLine($"Logging in as: {username}");
                UsernamesList.Add(new Username(username));
                Console.WriteLine($"Added account: {username}");
            }
        }

        public void RemoveUser(string username)
        {
            var userToRemove = UsernamesList.FirstOrDefault(user => user.Name == username);
            if (userToRemove != null)
            {
                Console.WriteLine($"Logged out: {username}");
                UsernamesList.Remove(userToRemove);
            }
            else
            {
                Console.WriteLine($"Username not found: {username}");
            }
        }

        /*------------------- Chatroom Management -------------------*/

        public List<Chatroom> CreateChatroom(string roomName, List<Chatroom> chatrooms)
        {
            if (!ChatroomsList.Any(room => room.RoomName == roomName))
            {
                ChatroomsList.Add(new Chatroom(roomName));
                Console.WriteLine($"New chat room created: {roomName}");
            }
            else
            {
                Console.WriteLine($"Chat room with the same name already exists: {roomName}");
            }
            return ChatroomsList;
        }

        public List<Chatroom> JoinChatroom(string roomName, string username, List<Chatroom> chatrooms)
        {
            var chatroom = ChatroomsList.FirstOrDefault(room => room.RoomName == roomName);
            if (chatroom != null)
            {
                Console.WriteLine($"Found chat room: {chatroom.RoomName}");
                if (!chatroom.Participants.Contains(username))
                {
                    chatroom.Participants.Add(username);
                    Console.WriteLine($"Added participant: {username}");
                }
                else
                {
                    Console.WriteLine($"Participant already exists: {username}");
                }
            }
            else
            {
                Console.WriteLine($"Chat room not found: {roomName}");
            }
            return ChatroomsList;
        }

        public List<Chatroom> LeaveChatroom(string roomName, string username, List<Chatroom> chatrooms)
        {
            var chatroom = ChatroomsList.FirstOrDefault(room => room.RoomName == roomName);
            if (chatroom != null)
            {
                if (chatroom.Participants.Contains(username))
                {
                    chatroom.Participants.Remove(username);
                    Console.WriteLine($"Removed participant from chat room: {username}");
                }
                else
                {
                    Console.WriteLine($"Participant is not in the chat room: {username}");
                }
            }
            return ChatroomsList.Where(room => room.Participants.Contains(username) || !room.IsPrivate).ToList();
        }

        public List<Chatroom> CreateInitialChatrooms(List<Chatroom> chatrooms)
        {
            for (int i = 1; i <= 3; i++)
            {
                string roomName = $"Chatroom {i}";
                if (!ChatroomsList.Any(room => room.RoomName == roomName))
                {
                    ChatroomsList.Add(new Chatroom(roomName));
                    Console.WriteLine($"Added initial Chatroom: {roomName}");
                }
            }
            return ChatroomsList;
        }

        public Chatroom GetChatroom(string roomName, List<Chatroom> chatrooms)
        {
            var chatroom = chatrooms.FirstOrDefault(room => room.RoomName == roomName);
            if (chatroom != null)
            {
                Console.WriteLine(chatroom.RoomName);
            }
            return chatroom;
        }

        /*------------------- Chatroom Retrieval -------------------*/

        public List<Chatroom> eGetChatroom(string username)
        {
            return ChatroomsList.Where(room => room.Participants.Contains(username) || !room.IsPrivate).ToList();
        }

        public List<Chatroom> iGetChatroom() // Consider renaming
        {
            return ChatroomsList;
        }

        /*------------------- Private Chatroom Management -------------------*/

        public List<Chatroom> CreatePrivateChatroom(string sender, string receiver)
        {
            string privateRoomName = $"{sender}_{receiver}";
            if (!ChatroomsList.Any(room => room.RoomName == privateRoomName))
            {
                var privateChatroom = new Chatroom(privateRoomName) { IsPrivate = true };
                privateChatroom.Participants.Add(sender);
                privateChatroom.Participants.Add(receiver);

                ChatroomsList.Add(privateChatroom);
                Console.WriteLine($"Created private chat room: {privateRoomName} for {sender} and {receiver}");
            }
            return ChatroomsList;
        }

        /*------------------- Messaging -------------------*/

        public List<Chatroom> SendMessage(string sender, string roomName, string message, List<Chatroom> chatrooms)
        {
            var chatroom = ChatroomsList.FirstOrDefault(room => room.RoomName == roomName);
            if (chatroom != null)
            {
                if (chatroom.Participants.Contains(sender))
                {
                    chatroom.Messages.Add(new Message { Sender = sender, Content = message });
                    Console.WriteLine($"Sender: {sender}\nMessage: {message}");
                }
                else
                {
                    Console.WriteLine($"Sender is not a participant in the chat room: {sender}");
                }
            }
            else
            {
                Console.WriteLine($"Chat room not found: {roomName}");
            }
            return ChatroomsList;
        }

        public void SendPrivateMessage(string sender, string receiver, string message)
        {
            string privateRoomName = $"{sender}_{receiver}";
            var privateChatroom = ChatroomsList.FirstOrDefault(room => room.RoomName == privateRoomName);

            if (privateChatroom == null)
            {
                CreatePrivateChatroom(sender, receiver);
                privateChatroom = ChatroomsList.FirstOrDefault(room => room.RoomName == privateRoomName);
            }

            if (privateChatroom != null)
            {
                privateChatroom.Messages.Add(new Message { Sender = sender, Content = message });
                Console.WriteLine($"{sender} sent a private message to {receiver}: {message}");
            }
        }

        /*------------------- File Handling -------------------*/

        public string UploadFile(string filePath, byte[] fileData, string currentChatroom)
        {
            
            string uploadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\GameLobbyServer\bin\Debug");

            
            Directory.CreateDirectory(uploadDirectory);

            
            string fileName = Path.GetFileName(filePath);

            
            string fullPath = Path.Combine(uploadDirectory, fileName);

            
            File.WriteAllBytes(fullPath, fileData);

            
            var chatroom = GetChatroom(currentChatroom, ChatroomsList);
            if (chatroom != null)
            {
                chatroom.Files.Add(fileName);  
                Console.WriteLine($"Successfully uploaded file: {fileName} to chatroom: {currentChatroom}");
            }

            return fileName;
        }

    }
}
