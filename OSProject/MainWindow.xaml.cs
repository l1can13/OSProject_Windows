using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OSProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AuthWindow authWindow = new AuthWindow();
            if(authWindow.ShowDialog() == true)
            {

            }

            sideMenu.SelectedItem = filesItem;

            List<string> filenamesList = new List<string>();
            filenamesList.Add("hello1");
            filenamesList.Add("hello2");
            filenamesList.Add("hello3");
            filenamesList.Add("hello4");
            filenamesList.Add("hello5");
            listViewFiles.ItemsSource = filenamesList;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
        }
    }
}
