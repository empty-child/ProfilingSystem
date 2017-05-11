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
using System.Drawing;
using SocialNetworksLibrary;
using Microsoft.Expression.Encoder.Devices;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.WPF;
using DirectShowLib;

namespace TestGUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Capture _capture = null;
        private static readonly CascadeClassifier Classifier = new CascadeClassifier(@"haarcascade_frontalface_default.xml");
        private int _frameCounter = 0;

        public MainWindow()
        {
            InitializeComponent();

            InitCapture();
            

        }

        void InitCapture()
        {
            CvInvoke.UseOpenCL = false;

            DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (_SystemCamereas.Length == 0)
            {
                MessageBox.Show("Camera not found");
                return;
            }
            _capture = new Capture();
            _capture.FlipHorizontal = true;
            //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 25);

            this.Dispatcher.Hooks.DispatcherInactive += Hooks_DispatcherInactive;

            _capture.Start();
            InitCortex();

        }

        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            using (Image<Bgr, Byte> frame = _capture.QueryFrame().ToImage<Bgr, Byte>())
            {
                if (frame != null)
                {
                    Image<Gray, byte> grayImage = frame.Convert<Gray, byte>();
                    Rectangle[] rectangles = Classifier.DetectMultiScale(grayImage, 1.1, 10);
                    if (rectangles.Length > 0) _frameCounter++;
                    else _frameCounter = 0;
                    if (_frameCounter == 50)
                    {
                        frame.Save("face_example.jpg");
                        _capture.Stop();
                        InitCortex();
                        this.Dispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
                    }
                    foreach (var face in rectangles)
                    {
                        frame.Draw(face, new Bgr(System.Drawing.Color.Green), 3);
                    }
                    image1.Source = BitmapSourceConvert.ToBitmapSource(frame);
                }
            }
        }

        void InitCortex()
        {
            VisualСortex vc = new VisualСortex("face_example.jpg");
            string id = vc.FindPerson();
            Init();
            Analysis();
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

        void Analysis()
        {
            PGPI a = new PGPI("afanasov_p");
            var data = a.Init();
            foreach(var key in data.Keys)
            {
                textbox1.Text += key+" с точностью "+data[key]+"\n";
                
            }
        }

    }
}
