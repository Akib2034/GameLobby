using GameLobbyServer;
using GameLobbyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace GameLobbyUI
{
    /// <summary>
    /// Interaction logic for FileDisplay.xaml
    /// </summary>
    public partial class FileDisplay : Window
    {
        public Chatroom currentChatroom = new Chatroom("Test");
        private List<string> fileNamesList = new List<string>();

        public FileDisplay() // constructor for the filedisplay window
        {
            InitializeComponent(); // initializes the component
        }

        public void setChatroom(Chatroom userRoom) // sets the chatroom and updates the file list
        {
            FileSelectionLb.ItemsSource = null;
            FileSelectionLb.ItemsSource = userRoom.Files;
        }

        private void FileSelectionLb_Selection(object sender, SelectionChangedEventArgs e)
        {
            if (FileSelectionLb.SelectedItem == null)
                return; 

            string selectedFile = FileSelectionLb.SelectedItem.ToString();

            
            TextFileDisplay.Text = "";
            ImageFileDisplay.Source = null;

            
            string relativePath = @"..\..\..\GameLobbyServer\bin\Debug\";
            string filePath = Path.Combine(relativePath, selectedFile); 

            
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"File not found: {filePath}");
                return;
            }

            
            if (Path.GetExtension(selectedFile) == ".txt")
            {
               
                TextFileDisplay.Text = File.ReadAllText(filePath);
            }
            else
            {
                // Display image
                try
                {
                    
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; 
                    bitmap.EndInit();

                
                    ImageFileDisplay.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}");
                }
            }
        }

    }
}
