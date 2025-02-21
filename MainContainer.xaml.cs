using System.Windows;
using System.Windows.Controls;

namespace AttendanceApp
{
    public partial class MainContainer : Window
    {
        public static Frame AppFrame { get; private set; }
        public MainContainer()
        {
            InitializeComponent();
            // Get the available work area (excluding taskbar)
            // Get the available work area (excluding taskbar)
            var window = Window.GetWindow(this);
            if (window != null)
            {
                var workArea = SystemParameters.WorkArea;
                window.Left = workArea.Left;
                window.Top = workArea.Top;
                window.Width = workArea.Width;
                window.Height = workArea.Height;
            }

            AppFrame = MainFrame;
            MainFrame.Navigate(new MainDashboard());

        }
    }
}
