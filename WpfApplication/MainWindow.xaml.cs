using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace WpfApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _icon;

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _icon = new NotifyIcon();
            _icon.Icon = Resource1.FileDown;
            _icon.MouseDoubleClick += _icon_MouseDoubleClick;
            _icon.MouseClick += ShowLastBalloon;
            _txtPasswort.Focus();

        }

        public event Action<string> PasswordReceived = s => {};

        void _icon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void _txtPasswort_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                toggleWindowState();
                PasswordReceived(_txtPasswort.Password);
            }
        }

        private void toggleWindowState()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                _icon.Visible = false;
                ShowInTaskbar = true;
            }
            else if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
                ShowTaskbarNotification("Minimiert", "App wurde minimiert");
            }
        }

        public void ShowTaskbarNotification(string title, string text)
        {
            ShowTaskbarNotification(title, text, 5);
        }

        public void ShowTaskbarNotification(string title, string text, int timeout)
        {
            _icon.BalloonTipTitle = title;
            _icon.BalloonTipText = text;
            _icon.Visible = true;

            _icon.ShowBalloonTip(timeout);
        }

        private void ShowLastBalloon(object sender, MouseEventArgs e)
        {
            _icon.ShowBalloonTip(5);
        }
    }
}
