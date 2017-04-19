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
using VK;

namespace TestGUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        public string Cache { get; set; }
        public string Token { get; set; }

        public void Init()
        {
            Authenticate auth = new Authenticate();
            if (auth.AccessToken == "" || auth.AccessToken == null)
            {
                auth.Scope = "offline,friends,wall,groups";
                if (auth.GetAuthData() != null)
                {
                    Cache = auth.GetAuthData();
                }
                Authorize AuthenticateWindow = new Authorize(Cache);
                AuthenticateWindow.ShowDialog();
                Token = AuthenticateWindow.Token;
                auth.WriteAccessToken(Token);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = Methods.UsersGet(new string[] { "210700286" }, new string[] { "city", "verified" });
            textBlock1.Text = "";
            foreach (List<string> items in result)
            {
                foreach (string item in items)
                {
                    textBlock1.Text += string.Concat(item, "\n");
                }
            }
        }
    }
}
