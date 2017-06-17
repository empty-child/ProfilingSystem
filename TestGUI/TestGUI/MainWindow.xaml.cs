using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using SocialNetworksLibrary;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.WPF;
using DirectShowLib;
using System.Runtime.InteropServices;

namespace CoreCVandUI
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

        private bool marker = false;
        public string Cache { get; set; }
        public string Token { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            //Analysis();
            DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            WebCams = new Video_Device[_SystemCamereas.Length];
            for (int i = 0; i < _SystemCamereas.Length; i++)
            {
                WebCams[i] = new Video_Device(i, _SystemCamereas[i].Name, _SystemCamereas[i].ClassID); //fill web cam array
                Camera_Selection.Items.Add(WebCams[i].ToString());
            }
            if (Camera_Selection.Items.Count > 0)
            {
                Camera_Selection.SelectedIndex = 0; //Set the selected device the default
            }

            InitCapture(0);
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
                        var handle = GCHandle.Alloc(grayImage);
                        Rectangle[] rectangles = Classifier.DetectMultiScale(grayImage, 1.1, 10);
                        handle.Free();
                        if (rectangles.Length > 0) _frameCounter++;
                        else _frameCounter = 0;
                        if (_frameCounter == 50)
                        {
                            frame.Save("face_example.jpg");
                            _capture.Stop();
                            this.Dispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
                            InitCortex();
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

        async void InitCortex()
        {
            VisualСortex vc = new VisualСortex("face_example.jpg");
            string id = await vc.FindPerson();
            if (id == null) return;
            AuthDialog();
            Analysis(id);
        }

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

        async void Analysis(string id = null)
        {
            if (id == null) id = "26405846";
            PGPI data = new PGPI(id, "city,home_town,universities,schools,sex,country,career,bdate");
            int depth = 1;
            if (depthBox.Text != null || depthBox.Text != "")
            {
                depth = Convert.ToInt32(depthBox.Text);
            }
            Loading l1 = new Loading();
            l1.Show();
            Dictionary<string, object> superUserData = await data.Init(depth);
            l1.Close();
            if (superUserData["photo"] != null && superUserData["name"] != null)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(superUserData["photo"].ToString(), UriKind.Absolute);
                bitmap.EndInit();
                nameTextBlock.Text = superUserData["name"].ToString();
                img1.Source = bitmap;
            }

            foreach (var group in (Dictionary<string, string>)superUserData["analysisResult"])
            {
                textAnalysisResultBox.Text += group.Value + "\n";
            }

            var output = new Dictionary<string, double>();
            var result = await data.InitPGPIN();
            //Outputdata(result);

            result = await data.InitPGPIG();
            Outputdata(result);
        }

        void Outputdata(Dictionary<string, Dictionary<string, double>> result)
        {
            var output = new Dictionary<string, double>();

            foreach (var key in result.Keys)
            {
                if (key.StartsWith("schools"))
                {
                    switch (key)
                    {
                        case "schools.name":
                            mEducationDataTextBox.Text += "Школа: ";
                            mEducationDataTextBox.Text += maxValue(key, result);
                            break;
                        case "schools.year_graduated":
                            mEducationDataTextBox.Text += "Выпуск: ";
                            mEducationDataTextBox.Text += maxValue(key, result);
                            break;
                    }
                }
                else if (key.StartsWith("universities"))
                {
                    switch (key)
                    {
                        case "universities.name":
                            hEducationDataTextBox.Text += "Университет: ";
                            hEducationDataTextBox.Text += maxValue(key, result);
                            break;
                        case "universities.faculty_name":
                            hEducationDataTextBox.Text += "Факультет: ";
                            hEducationDataTextBox.Text += maxValue(key, result);
                            break;
                        case "universities.chair_name":
                            hEducationDataTextBox.Text += "Кафедра: ";
                            hEducationDataTextBox.Text += maxValue(key, result);
                            break;
                        case "universities.graduation":
                            hEducationDataTextBox.Text += "Выпуск: ";
                            hEducationDataTextBox.Text += maxValue(key, result);
                            break;
                    }
                }
                else if (key.StartsWith("career"))
                {
                    switch (key)
                    {
                        case "career.company":
                            careerDataTextBox.Text += "Компания: ";
                            careerDataTextBox.Text += maxValue(key, result);
                            break;
                        case "career.position":
                            careerDataTextBox.Text += "Должность: ";
                            careerDataTextBox.Text += maxValue(key, result);
                            break;
                    }
                }
                else
                {
                    switch (key)
                    {
                        case "sex":
                            mainDataTextBox.Text += "Пол: ";
                            mainDataTextBox.Text += maxValue(key, result);
                            break;
                        case "bdate":
                            mainDataTextBox.Text += "Год рождения: ";
                            mainDataTextBox.Text += maxValue(key, result);
                            break;
                        case "city.title":
                            mainDataTextBox.Text += "Город: ";
                            mainDataTextBox.Text += maxValue(key, result);
                            break;
                        case "country.title":
                            mainDataTextBox.Text += "Страна: ";
                            mainDataTextBox.Text += maxValue(key, result);
                            break;
                        case "home_town":
                            mainDataTextBox.Text += "Родной город: ";
                            mainDataTextBox.Text += maxValue(key, result);
                            break;
                    }
                }

            }

            //foreach (var key in output.Keys)
            //{
            //    textbox1.Text += key + " с точностью " + output[key] + "\n";
            //}
        }

        string maxValue(string key, Dictionary<string, Dictionary<string, double>> result)
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
            return outkey + "\n";
        }

        private void Camera_Selection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (marker == false) InitCapture(Camera_Selection.SelectedIndex);
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings sWindow = new Settings();
            sWindow.Show();
        }

        private void captureControlButton_Click(object sender, RoutedEventArgs e)
        {
            if (marker == false)
            {
                _capture.Stop();
                _capture.Dispose();
                this.Dispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
                captureControlButton.Content = "Начать захват";
                marker = true;
                image1.Source = new BitmapImage(new Uri("resources/empty.png", UriKind.Relative));
            }
            else
            {
                InitCapture(Camera_Selection.SelectedIndex);
                captureControlButton.Content = "Остановить захват";
                marker = false;
            }
        }
    }
}
