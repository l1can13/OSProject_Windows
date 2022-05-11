using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Text.Json;
using Firebase.Auth;
using FireSharp;
using System.IO;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp.Response;
using System.Windows.Controls;

namespace OSProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FirebaseAuthProvider auth = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyApQwuz8TKJ6rZ9rqnBOt4vLOLCGAfvntI"));
        private Firebase.Auth.User fbAuth;
        private string token;
        private List<string> filenamesList = new List<string>();
        private FireSharp.Config.FirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "GCyOOQypSmKYS39cjXjV2KE9VjxoYvZ5WatyjjGk",
            BasePath = "https://galvanic-axle-343014-default-rtdb.firebaseio.com/"
        };

        public void saveList(List<string> filenamesList)
        {
            FirebaseClient client = new FirebaseClient(config);
            client.Set<List<string>>("User_Data/" + fbAuth.LocalId, filenamesList);
        }
        public List<string> loadList()
        {
            List<string> list = new List<string>();

            FirebaseClient client = new FirebaseClient(config);
            FirebaseResponse firebaseResponse = client.Get("User_Data/" + fbAuth.LocalId);
            list = firebaseResponse.ResultAs<List<string>>();

            return list;
        }

        public void setUser(string token)
        {
            fbAuth = auth.GetUserAsync(token).Result;
        }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                string fileName = "C:\\Users\\lican\\source\\repos\\OSProject_Windows\\OSProject\\token.json";
                string jsonString = File.ReadAllText(fileName);
                token = JsonSerializer.Deserialize<string>(jsonString);
            }
            catch (FileNotFoundException e)
            {
                AuthWindow authWindow = new AuthWindow();
                authWindow.ShowDialog();
                string fileName = "C:\\Users\\lican\\source\\repos\\OSProject_Windows\\OSProject\\token.json";
                string jsonString = File.ReadAllText(fileName);
                token = JsonSerializer.Deserialize<string>(jsonString);
            }
            catch (JsonException e)
            {
                AuthWindow authWindow = new AuthWindow();
                authWindow.ShowDialog();
                string fileName = "C:\\Users\\lican\\source\\repos\\OSProject_Windows\\OSProject\\token.json";
                string jsonString = File.ReadAllText(fileName);
                token = JsonSerializer.Deserialize<string>(jsonString);
            }

            setUser(token);

            if (token == null)
            {
                AuthWindow authWindow = new AuthWindow();
                authWindow.ShowDialog();
            }

            usernameTextBlock.Text = fbAuth.LocalId;
            userEmailTextBlock.Text = fbAuth.Email;

            sideMenu.SelectedItem = filesItem;

            filenamesList = loadList();
            listViewFiles.ItemsSource = filenamesList;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();

            FileCustom file = new FileCustom(openFileDialog.FileName, fbAuth);
            file.upload();
            if (filenamesList != null)
                filenamesList.Add(openFileDialog.SafeFileName);
            else
            {
                filenamesList = new List<string>();
                filenamesList.Add(openFileDialog.SafeFileName);
            }
            saveList(filenamesList);
            listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
            listViewFiles.ItemsSource = filenamesList;
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            FileCustom file = new FileCustom(listViewFiles.SelectedItem.ToString(), fbAuth);
            file.download();
        }

        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
            FileCustom file = new FileCustom(listViewFiles.SelectedItem.ToString(), fbAuth);
            string newFilename = Microsoft.VisualBasic.Interaction.InputBox("Введите текст:") + '.' + file.getTypeOfFile();
            file.rename(newFilename);
            int index = filenamesList.IndexOf(file.getFilename());
            filenamesList[index] = newFilename;

            saveList(filenamesList);
            listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
            listViewFiles.ItemsSource = filenamesList;
        }

        private void shareButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            FileCustom file = new FileCustom(listViewFiles.SelectedItem.ToString(), fbAuth);
            file.delete();
            filenamesList.Remove(file.getFilename());
            saveList(filenamesList);
            listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
            listViewFiles.ItemsSource = filenamesList;
        }
    }
}
