using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace JumpKingRavensMod.Settings.Editor
{
    /// <summary>
    /// Interaction logic for SettingsEditor.xaml
    /// </summary>
    public partial class SettingsEditor : Window
    {
        public SettingsEditor()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (tabControl != null)
            {
                if (tabControl.SelectedItem == tabItemYouTube)
                {
                    tabControl.SelectedItem = tabItemTwitch;
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tabControl != null)
            {
                if (tabControl.SelectedItem == tabItemTwitch)
                {
                    tabControl.SelectedItem = tabItemYouTube;
                }
            }
        }
    }
}
