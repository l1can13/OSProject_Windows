using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Text.Json;
using Firebase.Auth;
using FireSharp;
using System.IO;
using FireSharp.Response;
using System.Windows.Controls;
using System;
using System.Diagnostics;

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
            openFileDialog.Multiselect = true;
            openFileDialog.ShowDialog();

            var filesForUpload = openFileDialog.FileNames;
            var filesSafe = openFileDialog.SafeFileNames;

            for (int i = 0; i < filesForUpload.Length; ++i)
            {
                FileCustom file = new FileCustom(filesForUpload[i], fbAuth);
                file.upload();
                if (filenamesList != null)
                    filenamesList.Add(filesSafe[i]);
                else
                {
                    filenamesList = new List<string>();
                    filenamesList.Add(filesSafe[i]);
                }
            }
            
            saveList(filenamesList);
            listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
            listViewFiles.ItemsSource = filenamesList;
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            var files = listViewFiles.SelectedItems;

            for (int i = 0; i < files.Count; ++i)
            {
                FileCustom file = new FileCustom(files[i].ToString(), fbAuth);
                file.download();
            }

            Process.Start("explorer", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing");
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

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            filenamesList = loadList();
            listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
            listViewFiles.ItemsSource = filenamesList;
        }
    }
}
