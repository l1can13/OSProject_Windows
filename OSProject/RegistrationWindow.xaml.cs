using Firebase.Auth;
using FireSharp;
using System;
using System.Collections.Generic;
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

namespace OSProject
{
    public partial class RegistrationWindow : Window
    {
        FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyApQwuz8TKJ6rZ9rqnBOt4vLOLCGAfvntI"));
        private static FirebaseAuth fbAuth;
        private FireSharp.Config.FirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "GCyOOQypSmKYS39cjXjV2KE9VjxoYvZ5WatyjjGk",
            BasePath = "https://galvanic-axle-343014-default-rtdb.firebaseio.com/"
        };

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void already_have_account_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            AuthWindow authWindow = new AuthWindow();
            authWindow.ShowDialog();
        }

        private async void create_account_Click(object sender, RoutedEventArgs e)
        {
            await auth.CreateUserWithEmailAndPasswordAsync(email.Text, password.Text);

            FirebaseClient client = new FirebaseClient(config);
            var user = auth.SignInWithEmailAndPasswordAsync(email.Text, password.Text).Result;

            client.Set<string>("User_Info/" + user.User.LocalId + "/email", email.Text);
            client.Set<string>("User_Info/" + user.User.LocalId + "/id", user.User.LocalId);
            client.Set<string>("User_Info/" + user.User.LocalId + "/phoneNumber", phonenumber.Text);
            client.Set<string>("User_Info/" + user.User.LocalId + "/username", username.Text);

            this.Close();
            MainWindow mainWindow = new MainWindow();
            mainWindow.ShowDialog();
        }
    }
}
