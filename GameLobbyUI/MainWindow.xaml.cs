
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Reflection.Emit;
using System.Windows.Threading;

using GameLobbyServer;
using GameLobbyLib;

namespace GameLobbyUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServerInterface foob;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ChannelFactory<ServerInterface> foobFactory;
            NetTcpBinding tcp = new NetTcpBinding();
            tcp.TransferMode = TransferMode.Streamed;

            string URL = "net.tcp://localhost:8200/DataService";
            foobFactory = new ChannelFactory<ServerInterface>(tcp, URL);
            foob = foobFactory.CreateChannel();

            ChatroomsLb.ItemsSource = foob.CreateInitialChatrooms(new List<Chatroom>()).Select(room => room.RoomName);

            Task.Run(() => sUpdateChatrooms());
            Task.Run(() => sUpdateChatroomUsers());
            Task.Run(() => sUpdateChatroomMessages());
        }

        /*------------------- User Management -------------------*/

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = UserTB.Text;

            bool usernameExists = foob.CheckUsername(username);

            if (usernameExists)
            {
                MessageBox.Show("User already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter username", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else
            {
                foob.CreateUser(username);
                UsernameTb.Text = "Currently logged in as: " + username;
                LoginBtn.IsEnabled = false;
                UserTB.IsEnabled = false;
            }
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = UserTB.Text;
            bool usernameExists = foob.CheckUsername(username);

            if (usernameExists)
            {
                List<Chatroom> iChatroomsList = foob.eGetChatroom(username);
                bool isParticipantInChatroom = iChatroomsList.Any(room => room.Participants.Contains(username));

                if (isParticipantInChatroom)
                {
                    MessageBox.Show("You cannot log out while you are in a chat room, Please leave the chat room first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    foob.RemoveUser(username);
                    UsernameTb.Text = "You have been logged out: " + username;
                    UserTB.Text = "Enter a unique username to log in again";
                    LoginBtn.IsEnabled = true;
                    UserTB.IsEnabled = true;

                    ChatroomsLb.ItemsSource = null;
                    ChatTb.Text = string.Empty;
                    ChatroomUsersLv.ItemsSource = null;
                    ChatroomNameTb.Text = string.Empty;
                }
            }
            else
            {
                MessageBox.Show("User is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /*------------------- Chatroom Management -------------------*/

        private void CreateChatroomBtn_Click(object sender, RoutedEventArgs e)
        {
            string roomName = CreateChatroomTb.Text;
            bool usernameExists = foob.CheckUsername(UserTB.Text);

            if (usernameExists)
            {
                List<Chatroom> iChatroomsList = foob.eGetChatroom(UserTB.Text);

                if (iChatroomsList == null)
                {
                    MessageBox.Show("Failed to retrieve chat rooms", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (iChatroomsList.Any(room => room.RoomName == roomName))
                {
                    MessageBox.Show("Failed, chat room with that name already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    iChatroomsList = foob.CreateChatroom(roomName, iChatroomsList);

                    if (iChatroomsList != null)
                    {
                        iChatroomsList = foob.eGetChatroom(UserTB.Text);

                        ChatroomsLb.ItemsSource = null;
                        ChatroomsLb.ItemsSource = iChatroomsList.Select(room => room.RoomName);
                    }
                    else
                    {
                        MessageBox.Show("Failed to create chat room", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed, user is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChatroomsLb_Selection(object sender, SelectionChangedEventArgs e)
        {
            string username = null;
            username = UserTB.Text;

            if (!string.IsNullOrEmpty(username))
            {
                if (ChatroomsLb.SelectedItem != null)
                {
                    string selectedRoom = ChatroomsLb.SelectedItem.ToString();

                    List<Chatroom> iChatroomsList = foob.eGetChatroom(UserTB.Text);

                    if (iChatroomsList != null)
                    {
                        Chatroom userCurrentRoom = iChatroomsList.FirstOrDefault(room => room.Participants.Contains(username));

                        if (userCurrentRoom != null && !userCurrentRoom.IsPrivate)
                        {
                            MessageBox.Show("You are already a participant in a different room, Please leave the current room if you want to join this one", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            iChatroomsList = foob.JoinChatroom(selectedRoom, username, iChatroomsList);

                            Chatroom selectedChatroom = iChatroomsList.FirstOrDefault(room => room.RoomName == selectedRoom);

                            if (selectedChatroom != null)
                            {
                                ChatroomNameTb.Text = selectedChatroom.RoomName;
                                Console.WriteLine("Participants: " + selectedChatroom.GetParticipantsAsString());
                                Console.WriteLine("Participants count: " + selectedChatroom.Participants.Count);
                                ChatroomUsersLv.ItemsSource = selectedChatroom.Participants;
                            }
                            else
                            {
                                Console.WriteLine("Selected chat room not found in ChatroomsList");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to retrieve chat rooms", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                    ChatroomsLb.SelectedIndex = -1;
                    MessageBox.Show("Failed, user is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LeaveChatroomBtn_Click(object sender, RoutedEventArgs e)
        {
            bool usernameExists = foob.CheckUsername(UserTB.Text);

            if (usernameExists)
            {
                string roomName = ChatroomNameTb.Text;
                string username = UserTB.Text;

                List<Chatroom> iChatroomsList = foob.eGetChatroom(UsernameTb.Text);

                iChatroomsList = foob.LeaveChatroom(roomName, username, iChatroomsList);

                if (iChatroomsList != null)
                {
                    ChatroomNameTb.Text = string.Empty;
                    ChatroomUsersLv.ItemsSource = null;

                    ChatroomsLb.ItemsSource = null;
                    ChatroomsLb.ItemsSource = iChatroomsList.Select(room => room.RoomName);
                    ChatTb.Clear();
                }
                else
                {
                    MessageBox.Show("Failed to leave the chat room", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("User is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /*------------------- Messaging -------------------*/

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string roomName = ChatroomNameTb.Text;
            string message = MessageTb.Text;

            List<Chatroom> iChatroomsList = foob.eGetChatroom(UserTB.Text);

            bool usernameExists = foob.CheckUsername(UserTB.Text);

            if (usernameExists)
            {
                Chatroom userChatroom = iChatroomsList.FirstOrDefault(room => room.Participants.Contains(UserTB.Text));

                if (userChatroom != null)
                {
                    List<Chatroom> updatedChatroomsList = foob.SendMessage(UserTB.Text, roomName, message, iChatroomsList);

                    if (updatedChatroomsList != null)
                    {
                        Chatroom updatedChatroom = updatedChatroomsList.FirstOrDefault(room => room.RoomName == roomName);

                        if (updatedChatroom != null)
                        {
                            ChatTb.Text = string.Join(Environment.NewLine, updatedChatroom.Messages.Select(msg => $"{msg.Sender}: {msg.Content}"));
                            MessageTb.Clear();

                            iChatroomsList = updatedChatroomsList;
                        }
                        else
                        {
                            MessageBox.Show("Chat room not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to send message", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("You are not in a chat room", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Failed, user is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /*------------------- File Sharing -------------------*/

        private void UploadFileBtn_Click(object sender, RoutedEventArgs e)
        {
            bool usernameExists = foob.CheckUsername(UserTB.Text);
            List<Chatroom> iChatroomsList = foob.eGetChatroom(UserTB.Text);

            if (usernameExists)
            {
                Chatroom userChatroom = iChatroomsList.FirstOrDefault(room => room.Participants.Contains(UserTB.Text));

                if (userChatroom != null) { 
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Filter = "txt files |*.txt|Image files|*.jpg;*.jpeg;*.png;*.bmp";
                    dialog.CheckFileExists = true;
                    dialog.CheckPathExists = true;
                    dialog.Title = "Select file to upload";
                    dialog.ShowDialog();

                    string filePath = dialog.FileName;
                    string username = UserTB.Text;
                    string userCurrentRoom = ChatroomNameTb.Text;
                    if (filePath != "") { 
                        try { 
                            byte[] fileData = File.ReadAllBytes(filePath);

                            MessageTb.Text = username + ": sent file " + foob.UploadFile(filePath, fileData, userCurrentRoom);
                            foob.SendMessage(username, userCurrentRoom, MessageTb.Text, foob.iGetChatroom());
                            MessageTb.Text = "";
                        } catch (IOException exc)
                        {
                            MessageBox.Show("File transfer failed due to " +  exc.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                } 
                else{
                    MessageBox.Show("You are not in a chat room", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("User is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ChatroomFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            FileDisplay fileWindow = new FileDisplay();
            string username = UserTB.Text;
            string listonames = "";

            bool usernameExists = foob.CheckUsername(UserTB.Text);
            if (usernameExists)
            {
                Chatroom userCurrentRoom = (Chatroom)foob.GetChatroom(ChatroomNameTb.Text, foob.eGetChatroom(username));
                if (userCurrentRoom != null)
                {

                    if (userCurrentRoom.Files != null)
                    {
                        int count = userCurrentRoom.Files.Count;
                        listonames = count.ToString();
                        foreach (string file in userCurrentRoom.Files)
                        {

                            listonames += file + "\n";
                        }

                        fileWindow.setChatroom(userCurrentRoom);
                        fileWindow.Show();
                    }
                }
                else
                {
                    MessageBox.Show("You are not in a chat room", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("User is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /*------------------- Private Messaging -------------------*/

        private void PrivateMessageBtn_click(object sender, RoutedEventArgs e)
        {
            string senderUsername = UserTB.Text;
            string receiverUsername = PrivateMessageTb.Text;
            string message = MessageTb.Text;

            bool usernameExists = foob.CheckUsername(UserTB.Text);

            if (usernameExists)
            {
                if (string.IsNullOrWhiteSpace(receiverUsername))
                {
                    MessageBox.Show("Please enter a recipient's username", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (senderUsername.Equals(receiverUsername, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("You cannot send a private message to yourself.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool receiverExists = foob.CheckUsername(receiverUsername);
                if (!receiverExists)
                {
                    MessageBox.Show("Recipient's username does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                foob.SendPrivateMessage(senderUsername, receiverUsername, message);

                PrivateMessageTb.Clear();
                MessageTb.Clear();
            }

            else
            {
                MessageBox.Show("User is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /*------------------- Refresh Managment -------------------*/

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            string senderUsername = null;
            senderUsername = UserTB.Text;
            List<Chatroom> iChatroomsList = foob.eGetChatroom(senderUsername);

            bool usernameExists = foob.CheckUsername(senderUsername);

            if (usernameExists)
            {
                if (iChatroomsList != null)
                {
                    string ChatroomName = ChatroomNameTb.Text;
                    Chatroom selectedChatroom = iChatroomsList.FirstOrDefault(room => room.RoomName == ChatroomName);
                    ChatroomsLb.ItemsSource = iChatroomsList.Select(room => room.RoomName);

                    if (selectedChatroom != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ChatroomUsersLv.ItemsSource = selectedChatroom.Participants;
                            ChatTb.Text = string.Join(Environment.NewLine, selectedChatroom.Messages.Select(msg => $"{msg.Sender}: {msg.Content}"));
                        });
                    }
                }
            }
            else
            {
                MessageBox.Show("User is not logged in", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /*------------------- Background Tasks -------------------*/

        private async Task sUpdateChatrooms()
        {
            while (true)
            {
                await UpdateChatrooms();
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private async Task UpdateChatrooms()
        {
            string username = null;

            Dispatcher.Invoke(() =>
            {
                username = UserTB.Text;
            });

            try
            {
                if (!string.IsNullOrEmpty(username))
                {
                    List<Chatroom> iChatroomsList = await Task.Run(() => foob.eGetChatroom(username));

                    if (iChatroomsList != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ChatroomsLb.ItemsSource = null;
                            ChatroomsLb.ItemsSource = iChatroomsList.Select(room => room.RoomName);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }
        private async Task sUpdateChatroomUsers()
        {
            while (true)
            {
                await UpdateChatroomUsers();
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }


        private async Task UpdateChatroomUsers()
        {
            string username = null;
            string selectedRoom = null;
            Dispatcher.Invoke(() =>
            {
                username = UserTB.Text;
                selectedRoom = ChatroomNameTb.Text;
            });
            if (!string.IsNullOrEmpty(username))
            {
                List<Chatroom> iChatroomsList = await Task.Run(() => foob.eGetChatroom(username));

                if (iChatroomsList != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Chatroom selectedChatroom = iChatroomsList.FirstOrDefault(room => room.RoomName == selectedRoom);
                        if (selectedChatroom != null)
                        {
                            ChatroomUsersLv.ItemsSource = null;
                            ChatroomUsersLv.ItemsSource = selectedChatroom.Participants;
                        }
                    });
                }
            }
        }

       private async Task sUpdateChatroomMessages()
            {
            while (true)
            {
                await UpdateChatroomMessagesAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        private async Task UpdateChatroomMessagesAsync()
        {
            string username = null;
            string selectedRoom = null;

            Dispatcher.Invoke(() =>
            {
                username = UserTB.Text;
                selectedRoom = ChatroomNameTb.Text;
            });

            if (!string.IsNullOrEmpty(username))
            {
                List<Chatroom> iChatroomsList = await Task.Run(() => foob.eGetChatroom(username));

                if (iChatroomsList != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Chatroom selectedChatroom = iChatroomsList.FirstOrDefault(room => room.RoomName == selectedRoom);
                        if (selectedChatroom != null)
                        {
                            ChatTb.Text = string.Join(Environment.NewLine, selectedChatroom.Messages.Select(msg => $"{msg.Sender}: {msg.Content}"));
                        }
                    });
                }
            }
        }

    }
}