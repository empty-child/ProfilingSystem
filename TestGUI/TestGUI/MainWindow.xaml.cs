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
            var result = Methods.UsersGet(new string[] { "***" }, new string[] { "photo_id" });
            string id = result[0]["id"].ToString();
            result = Methods.FriendsGet(id, "hints", "", new string[] { "first_name", "last_name", "home_town", "schools" });
            textBlock1.Text = "";
            List<string> targetIDs = new List<string>();
            foreach (Dictionary<string, object> items in result)
            {
                targetIDs.Add(items["id"].ToString());
            }
            var a = targetIDs.ToArray();
            result = Methods.FriendsGetMutual(id, a);
            //textBlock1.Text += string.Concat(item, "\n");
        }
    }
}
