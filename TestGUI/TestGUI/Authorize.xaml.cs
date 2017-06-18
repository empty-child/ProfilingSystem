using MSHTML;
using System;
using System.Windows;

namespace CoreCVandUI
{
    /// <summary>
    /// Логика взаимодействия для Authorize.xaml
    /// </summary>
    public partial class Authorize : Window
    {
        private string _token;

        public string Token
        {
            get { return _token; }
        }

        public Authorize(string link)
        {
            InitializeComponent();
            Init(link);
        }

        public void Init(string link)
        {
            wb1.Source = new Uri(link);
        }

        private void wb1_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            string[] SourceName = wb1.Source.ToString().Split(new char[] { '#', '/', '&', '=', ':', '?' }, StringSplitOptions.RemoveEmptyEntries);

            HTMLDocument doc = (HTMLDocument)wb1.Document;

            if (SourceName[6] == "page")
            {
                IHTMLElementCollection theElementCollection = doc.getElementsByName("email");
                foreach (IHTMLElement curElement in theElementCollection)
                {
                    curElement.setAttribute("value", Properties.Settings.Default.BotLogins[0]);
                }

                theElementCollection = doc.getElementsByName("pass");
                foreach (IHTMLElement curElement in theElementCollection)
                {
                    curElement.setAttribute("value", Properties.Settings.Default.BotPasswords[0]);
                    doc.getElementById("install_allow").click();
                }

                theElementCollection = doc.getElementsByClassName("flat_button fl_r button_indent");
                foreach (IHTMLElement curElement in theElementCollection)
                {
                    curElement.click();
                }
            }
            else if (SourceName[2] == "blank.html")
            {
                _token = SourceName[4];
                this.Close();
            }
        }
    }
}
