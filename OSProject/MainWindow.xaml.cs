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
using System.Linq;
using Newtonsoft.Json;

namespace OSProject
{
    public partial class MainWindow : Window
    {
        private FirebaseAuthProvider auth = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyApQwuz8TKJ6rZ9rqnBOt4vLOLCGAfvntI"));
        private Firebase.Auth.User fbAuth;
        private string token;
        private bool isTrash = false;
        private List<string> filenamesList = new List<string>();
        private List<string> trashList = new List<string>();
        private FireSharp.Config.FirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "GCyOOQypSmKYS39cjXjV2KE9VjxoYvZ5WatyjjGk",
            BasePath = "https://galvanic-axle-343014-default-rtdb.firebaseio.com/"
        };

        public void saveTrash()
        {
            FirebaseClient client = new FirebaseClient(config);
            client.Set<List<string>>("User_Data/" + fbAuth.LocalId + "/Trash", trashList);
        }

        public List<string> loadTrash()
        {
            List<string> list = new List<string>();

            FirebaseClient client = new FirebaseClient(config);
            FirebaseResponse firebaseResponse = client.Get("User_Data/" + fbAuth.LocalId + "/Trash");
            list = firebaseResponse.ResultAs<List<string>>();

            return list;
        }

        public void saveList(List<string> filenamesList)
        {
            FirebaseClient client = new FirebaseClient(config);
            client.Set<List<string>>("User_Data/" + fbAuth.LocalId + "/Current", filenamesList);
        }

        bool IsDigit(string str)
        {
            foreach (char elem in str)
            {
                if (!Char.IsDigit(elem))
                {
                    return false;
                }
            }

            return true;
        }

        public List<string> loadList()
        {
            List<string> list = new List<string>();

            try
            {
                FirebaseClient client = new FirebaseClient(config);
                FirebaseResponse firebaseResponse = client.Get("User_Data/" + fbAuth.LocalId + "/Current/");
                list = firebaseResponse.ResultAs<List<string>>();

                return list;
            }
            catch (Newtonsoft.Json.JsonSerializationException)
            {
                FirebaseClient client = new FirebaseClient(config);
                FirebaseResponse firebaseResponse = client.Get("User_Data/" + fbAuth.LocalId + "/Current/");

                string temp = firebaseResponse.Body;

                temp = temp.Insert(temp.IndexOf('['), "'").Insert(temp.IndexOf(']') + 2, "'");

                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(temp);

                foreach (var elem in dict)
                {
                    if (IsDigit(elem.Key))
                    {
                        list.Add(elem.Value);
                    }
                    else
                    {
                        list.Add(elem.Key);
                    }
                }

                return list;
            }

        }

        public string getUsername()
        {
            FirebaseClient client = new FirebaseClient(config);
            return client.Get("User_Info/" + fbAuth.LocalId + "/username").ResultAs<string>();
        }

        public void setUser(string token)
        {
            fbAuth = auth.GetUserAsync(token).Result;
        }

        public MainWindow()
        {
            InitializeComponent();
            isTrash = false;
            try
            {
                string fileName = "token.json";
                string jsonString = File.ReadAllText(fileName);
                token = System.Text.Json.JsonSerializer.Deserialize<string>(jsonString);
            }
            catch (FileNotFoundException e)
            {
                AuthWindow authWindow = new AuthWindow();
                authWindow.ShowDialog();
                string fileName = "token.json";
                string jsonString = File.ReadAllText(fileName);
                token = System.Text.Json.JsonSerializer.Deserialize<string>(jsonString);
            }
            catch (System.Text.Json.JsonException e)
            {
                AuthWindow authWindow = new AuthWindow();
                authWindow.ShowDialog();
                string fileName = "token.json";
                string jsonString = File.ReadAllText(fileName);
                token = System.Text.Json.JsonSerializer.Deserialize<string>(jsonString);
            }

            try
            {
                setUser(token);
            }
            catch (System.AggregateException e)
            {
                AuthWindow authWindow = new AuthWindow();
                authWindow.ShowDialog();
                string fileName = "token.json";
                string jsonString = File.ReadAllText(fileName);
                token = System.Text.Json.JsonSerializer.Deserialize<string>(jsonString);
                setUser(token);
            }

            if (token == null)
            {
                AuthWindow authWindow = new AuthWindow();
                authWindow.ShowDialog();
            }

            usernameTextBlock.Text = getUsername();
            userEmailTextBlock.Text = fbAuth.Email;

            trashList = loadTrash();
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
                if (file.getFilename().EndsWith("-folder"))
                {
                    file.download_folder();
                }
                else
                {
                    file.download(isTrash);
                }
            }

            Process.Start("explorer", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing");
        }

        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
            FileCustom file = new FileCustom(listViewFiles.SelectedItem.ToString(), fbAuth);
            string newFilename;
            if (file.getFilename().EndsWith("-folder"))
            {
                newFilename = Microsoft.VisualBasic.Interaction.InputBox("Введите текст:") + "-folder";
            }
            else
            {
                newFilename = Microsoft.VisualBasic.Interaction.InputBox("Введите текст:") + '.' + file.getTypeOfFile();
            }
            file.rename(newFilename, isTrash);
            int index = filenamesList.IndexOf(file.getFilename());
            filenamesList[index] = newFilename;

            saveList(filenamesList);
            listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
            listViewFiles.ItemsSource = filenamesList;
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> temp = listViewFiles.SelectedItems.Cast<string>().ToList();
            for (int i = 0; i < temp.Count; ++i)
            {
                FileCustom file = new FileCustom(temp[i], fbAuth);

                if (isTrash)
                {
                    file.delete(isTrash);
                    trashList.Remove(file.getFilename());

                    saveTrash();

                    listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
                    listViewFiles.ItemsSource = trashList;
                }
                else
                {
                    if (file.getFilename().EndsWith("-folder"))
                    {
                        file.delete_directory();

                        filenamesList.Remove(file.getFilename());
                        saveList(filenamesList);
                    }
                    else
                    {
                        file.move_to_trash();
                        filenamesList.Remove(file.getFilename());
                        saveList(filenamesList);

                        if (trashList != null)
                        {
                            trashList.Add(file.getFilename());
                        }
                        else
                        {
                            trashList = new List<string>();
                            trashList.Add(file.getFilename());
                        }
                        saveTrash();
                    }
                    listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
                    listViewFiles.ItemsSource = filenamesList;
                }
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            if (isTrash)
            {
                trashList = loadTrash();
                listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
                listViewFiles.ItemsSource = trashList;
            }
            else
            {
                filenamesList = loadList();
                listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
                listViewFiles.ItemsSource = filenamesList;
            }
        }

        private void listViewFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewFiles.SelectedItem != null)
            {
                FileCustom file = new FileCustom(listViewFiles.SelectedItem.ToString(), fbAuth);

                filenameTextBlock.Text = file.getFilename();
                typeOfFileTextBlock.Text = "Формат файла: " + file.getTypeOfFile();
            }
            else
            {
                filenameTextBlock.Text = "Название файла";
                typeOfFileTextBlock.Text = "Формат файла: ";
            }
        }

        private void filesButton_Click(object sender, RoutedEventArgs e)
        {
            titleTextBlock.Text = "Файлы";
            filenamesList = loadList();

            isTrash = false;

            backToFilesButton.Visibility = Visibility.Hidden;
            return_back_image.Visibility = Visibility.Hidden;

            renameButton.Visibility = Visibility.Visible;
            rename_image.Visibility = Visibility.Visible;

            listViewFiles.ItemsSource = filenamesList;
        }

        private void trashButton_Click(object sender, RoutedEventArgs e)
        {
            titleTextBlock.Text = "Корзина";
            trashList = loadTrash();

            isTrash = true;

            backToFilesButton.Visibility = Visibility.Visible;
            return_back_image.Visibility = Visibility.Visible;

            renameButton.Visibility = Visibility.Hidden;
            rename_image.Visibility = Visibility.Hidden;

            listViewFiles.ItemsSource = trashList;
        }

        private void backToFilesButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < listViewFiles.SelectedItems.Count; ++i)
            {
                FileCustom file = new FileCustom(listViewFiles.SelectedItems[i].ToString(), fbAuth);

                filenamesList.Add(file.getFilename());
                saveList(filenamesList);

                trashList.Remove(file.getFilename());
                saveTrash();

                file.rename(file.getFilename(), isTrash);
            }
            listViewFiles.ClearValue(ItemsControl.ItemsSourceProperty);
            listViewFiles.ItemsSource = trashList;
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "token.json";
            File.Delete(fileName);
            Environment.Exit(0);
        }

        private void findButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> temp = new List<string>();
            if (findBox.Text != "")
            {
                for (int i = listViewFiles.Items.Count - 1; i >= 0; --i)
                {
                    var item = listViewFiles.Items[i];
                    if (item.ToString().ToLower().Contains(findBox.Text.ToLower()))
                    {
                        temp.Add(item.ToString());
                    }
                }
                listViewFiles.ItemsSource = temp;
            }
            else
            {
                if (isTrash) listViewFiles.ItemsSource = trashList;
                else listViewFiles.ItemsSource = filenamesList;
            }
        }
    }
}
