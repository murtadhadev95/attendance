using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceApp
{
    public partial class LecturerLoginWindow : Page
    {
        private List<Lecturer> lecturers;

        public LecturerLoginWindow()
        {
            InitializeComponent();
            LoadLecturers();
        }

        private void LoadLecturers()
        {
            if (File.Exists("lecturers.json"))
            {
                var json = File.ReadAllText("lecturers.json");
                lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json) ?? new List<Lecturer>();
            }
            else
            {
                lecturers = new List<Lecturer>();
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = pbPassword.Password;

            // For simplicity, we use Lecturer.Name as the username.
            Lecturer lecturer = lecturers.FirstOrDefault(l => l.UserName == username && l.Password == password);
            if (lecturer != null)
            {


                MainContainer.AppFrame.Navigate(new LecturerMainPortal(lecturer));


            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

            MainContainer.AppFrame.Navigate(new MainDashboard());

        }
    }
}
