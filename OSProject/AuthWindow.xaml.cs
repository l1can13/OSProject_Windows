using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Firebase;
using Firebase.Auth;
using System.IO;

namespace OSProject
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyApQwuz8TKJ6rZ9rqnBOt4vLOLCGAfvntI"));
        private static FirebaseAuth fbAuth;
        public AuthWindow()
        {
            InitializeComponent();
            

        }

        public static FirebaseAuth getFbAuth()
        {
            return fbAuth;
        }

        public void SignIn(string userEmail, string userPassword)
        {
            try
            {
                fbAuth = auth.SignInWithEmailAndPasswordAsync(userEmail, userPassword).Result;
                string fileName = "C:\\Users\\lican\\source\\repos\\OSProject_Windows\\OSProject\\token.json";
                string jsonString = JsonSerializer.Serialize(fbAuth.FirebaseToken);
                File.WriteAllText(fileName, jsonString);
            }
            catch (FirebaseAuthException e)
            {
                MessageBox.Show("Неправильная почта или пароль!");
            }
        }

        private void authButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SignIn(emailBox.Text, passwordBox.Text);
                this.Close();
            }
            catch
            {
                MessageBox.Show("Нет подключения к интернету или проблема с серверами!");
            }
        }
    }
}
