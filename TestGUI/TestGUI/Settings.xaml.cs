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

namespace CoreCVandUI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            findFaceId.Text = CoreCVandUI.Properties.Settings.Default.FindFaceID;
            findFaceToken.Text = CoreCVandUI.Properties.Settings.Default.FindFaceToken;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            CoreCVandUI.Properties.Settings.Default.FindFaceID = findFaceId.Text;
            CoreCVandUI.Properties.Settings.Default.FindFaceToken = findFaceToken.Text;
            CoreCVandUI.Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
