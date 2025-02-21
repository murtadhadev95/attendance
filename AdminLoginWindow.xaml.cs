using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceApp
{
    public partial class AdminLoginWindow : Page
    {
        private string storedPassword = "admin"; // default; in a real app, load from file

        public AdminLoginWindow()
        {
            InitializeComponent();
            // Optionally load storedPassword from a JSON file, e.g., admin.json
            if (File.Exists("admin.json"))
            {
                var json = File.ReadAllText("admin.json");
                var adminData = JsonSerializer.Deserialize<AdminData>(json);
                if (adminData != null)
                    storedPassword = adminData.Password;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (pbAdminPassword.Password == storedPassword)
            {


                MainContainer.AppFrame.Navigate(new MainWindow());

            }
            else
            {
                MessageBox.Show("Incorrect password.");
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to MainDashboard or any previous page.
            MainContainer.AppFrame.Navigate(new MainDashboard());
        }
    }

    public class AdminData
    {
        public string Password { get; set; }
    }
}
