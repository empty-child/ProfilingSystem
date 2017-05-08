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
using SocialNetworksLibrary;
using Microsoft.Expression.Encoder.Devices;
using WebcamControl;

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
            //Init();
            PGPI a = new PGPI("afanasov_p");
            a.Init();

            //Binding binding_1 = new Binding("SelectedValue");
            //binding_1.Source = VideoDevicesComboBox;
            //WebcamCtrl.SetBinding(Webcam.VideoDeviceProperty, binding_1);

            //WebcamCtrl.FrameRate = 30;
            //WebcamCtrl.FrameSize = new System.Drawing.Size(640, 480);
            //var vidDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video);
            //VideoDevicesComboBox.ItemsSource = vidDevices;
            //VideoDevicesComboBox.SelectedIndex = 0;

        }

        private void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Display webcam video
                WebcamCtrl.StartPreview();
            }
            catch (Microsoft.Expression.Encoder.SystemErrorException ex)
            {
                MessageBox.Show("Device is in use by another application");
            }
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
            PGPI a = new PGPI("afanasov_p");
            a.Init();
            //textBlock1.Text = "";
            
            //textBlock1.Text += string.Concat(item, "\n");
        }
    }
}
