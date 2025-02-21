using System.Windows;
using System.Windows.Controls;

namespace AttendanceApp
{
    public partial class MainDashboard : Page
    {
        public MainDashboard()
        {
            InitializeComponent();
        }

        private void btnAdminPortal_Click(object sender, RoutedEventArgs e)
        {
            // Launch the Admin Login (which then leads to the Admin Portal)

            MainContainer.AppFrame.Navigate(new AdminLoginWindow());

        }

        private void btnLecturerPortal_Click(object sender, RoutedEventArgs e)
        {
            // Launch the Lecturer Login


            MainContainer.AppFrame.Navigate(new LecturerLoginWindow());

        }
    }
}
