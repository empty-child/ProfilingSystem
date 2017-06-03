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
        Video_Device[] WebCams;

        public MainWindow()
        {
            InitializeComponent();
            Analysis();

            //DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            //WebCams = new Video_Device[_SystemCamereas.Length];
            //for (int i = 0; i < _SystemCamereas.Length; i++)
            //{
            //    WebCams[i] = new Video_Device(i, _SystemCamereas[i].Name, _SystemCamereas[i].ClassID); //fill web cam array
            //    Camera_Selection.Items.Add(WebCams[i].ToString());
            //}
            //if (Camera_Selection.Items.Count > 0)
            //{
            //    Camera_Selection.SelectedIndex = 0; //Set the selected device the default
            //}

            //InitCapture(0);


        }

        void InitCapture(int Camera_Identifier)
        {
            if (_capture != null) _capture.Dispose();
            try
            {
                _capture = new Capture(Camera_Identifier);
                this.Dispatcher.Hooks.DispatcherInactive += Hooks_DispatcherInactive;
                _capture.Start();
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }


            //_capture.FlipHorizontal = true;
            //_capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 25);
            //InitCortex();

        }

        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            try
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
            catch
            {
                this.Dispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
                MessageBox.Show("Capture error");
                _capture.Stop();
                _capture.Dispose();
            }
        }

        void InitCortex()
        {
            VisualСortex vc = new VisualСortex("face_example.jpg");
            string id = vc.FindPerson();
            AuthDialog();
#if DEBUG
            id = null;
#endif
            Analysis(id);
        }

        public string Cache { get; set; }
        public string Token { get; set; }

        public void AuthDialog()
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

        void Analysis(string id = null)
        {
            if (id == null) id = "";
            PGPI data = new PGPI(id, "city,home_town,universities,schools,sex,country,career,bdate");
            int depth = 1;
            if (depthBox.Text != null || depthBox.Text != "")
            {
                depth = Convert.ToInt32(depthBox.Text);
            }
            Dictionary<string,string> superUserData = data.Init(depth);
            if(superUserData["photo"]!=null && superUserData["name"] != null)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(superUserData["photo"], UriKind.Absolute);
                bitmap.EndInit();
                nameTextBlock.Text = superUserData["name"];
                img1.Source = bitmap;
            }

            var output = new Dictionary<string, double>();
            var result = data.InitPGPIN();
            Outputdata(result);

            //result = a.InitPGPIG();
            //Outputdata(result);

        }

        void Outputdata(Dictionary<string, Dictionary<string, double>> result)
        {
            var output = new Dictionary<string, double>();

            foreach (var key in result.Keys)
            {
                double max = 0;
                string outkey = "";
                foreach (var innerkey in result[key].Keys)
                {
                    if (result[key][innerkey] > max)
                    {
                        outkey = innerkey;
                        max = result[key][innerkey];
                    }
                }
                output.Add(key + ": " + outkey, max);
            }

            foreach (var key in output.Keys)
            {
                textbox1.Text += key + " с точностью " + output[key] + "\n";
            }
        }

        private void Camera_Selection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitCapture(Camera_Selection.SelectedIndex);
        }
    }
}
