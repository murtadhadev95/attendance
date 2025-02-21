using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ExcelDataReader;
using System.Text;
using System.Text.Json;

namespace AttendanceApp
{
    public partial class MainWindow : Page
    {
        // Observable collections for ComboBox/ListBox binding.
        public ObservableCollection<string> Groups { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Stages { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Subjects { get; set; } = new ObservableCollection<string>();

        // In-memory lists for students and lecturers.
        private List<Student> students = new List<Student>();
        private List<Lecturer> lecturers = new List<Lecturer>();

        // Counters for auto-generated IDs.
        private int nextStudentId = 1;
        private int nextLecturerId = 1;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Load existing data from JSON files.
            LoadLists();
            LoadStudents();
            LoadLecturers();

            // Set auto-generated IDs.
            txtStudentID.Text = nextStudentId.ToString();
            txtLecturerID.Text = nextLecturerId.ToString();

            // Set ItemsSource for Lecturer List DataGrid.
            dgLecturers.ItemsSource = lecturers;

            // Initialize the "View Students" DataGrid.
            FilterStudents();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // When Back is clicked, return to the Main Dashboard.
            // Option 1: Close this window and re-show the existing MainDashboard.
            // Option 2: Create a new instance of MainDashboard.
            // Here we create a new instance for simplicity.
            MainContainer.AppFrame.Navigate(new MainDashboard());

        }
        #region JSON Load/Save Methods

        private void LoadLists()
        {
            // Load Groups
            if (File.Exists("groups.json"))
            {
                var groupsData = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("groups.json"));
                if (groupsData != null)
                {
                    Groups = new ObservableCollection<string>(groupsData);
                    lbGroups.ItemsSource = Groups;
                    cbStudentGroup.ItemsSource = Groups;
                }
            }
            // Load Stages
            if (File.Exists("stages.json"))
            {
                var stagesData = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("stages.json"));
                if (stagesData != null)
                {
                    Stages = new ObservableCollection<string>(stagesData);
                    lbStages.ItemsSource = Stages;
                    cbStudentStage.ItemsSource = Stages;
                    lbLecturerStages.ItemsSource = Stages;
                }
            }
            // Load Subjects
            if (File.Exists("subjects.json"))
            {
                var subjectsData = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("subjects.json"));
                if (subjectsData != null)
                {
                    Subjects = new ObservableCollection<string>(subjectsData);
                    lbSubjectsSetting.ItemsSource = Subjects;
                    lbLecturerSubjects.ItemsSource = Subjects;
                }
            }
        }

        private void LoadStudents()
        {
            if (File.Exists("students.json"))
            {
                var json = File.ReadAllText("students.json");
                students = JsonSerializer.Deserialize<List<Student>>(json) ?? new List<Student>();
                if (students.Any())
                    nextStudentId = students.Max(s => s.ID) + 1;
            }
        }

        private void LoadLecturers()
        {
            if (File.Exists("lecturers.json"))
            {
                var json = File.ReadAllText("lecturers.json");
                lecturers = JsonSerializer.Deserialize<List<Lecturer>>(json) ?? new List<Lecturer>();
                if (lecturers.Any())
                    nextLecturerId = lecturers.Max(l => l.ID) + 1;
            }
        }

        private void SaveStudents()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("students.json", JsonSerializer.Serialize(students, options));
        }

        private void SaveLecturers()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("lecturers.json", JsonSerializer.Serialize(lecturers, options));
        }

        private void SaveGroups()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("groups.json", JsonSerializer.Serialize(Groups.ToList(), options));
        }

        private void SaveStages()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("stages.json", JsonSerializer.Serialize(Stages.ToList(), options));
        }

        private void SaveSubjects()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("subjects.json", JsonSerializer.Serialize(Subjects.ToList(), options));
        }

        #endregion

        #region Event Handlers

        // Add a student manually.
        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentName.Text) ||
                cbStudentStage.SelectedItem == null ||
                cbStudentGroup.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all fields for the student.");
                return;
            }

            var student = new Student
            {
                ID = nextStudentId,
                Name = txtStudentName.Text,
                Stage = cbStudentStage.SelectedItem.ToString(),
                Group = cbStudentGroup.SelectedItem.ToString()
            };

            students.Add(student);
            SaveStudents();

            MessageBox.Show($"Student added:\nID: {student.ID}\nName: {student.Name}\nStage: {student.Stage}\nGroup: {student.Group}");

            nextStudentId++;
            txtStudentID.Text = nextStudentId.ToString();
            txtStudentName.Clear();

            // Update the View Students grid.
            FilterStudents();
        }

        // Add a lecturer manually.
        // In the AddLecturer_Click event handler:
        private void AddLecturer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLecturerName.Text))
            {
                MessageBox.Show("Please enter a name for the lecturer.");
                return;
            }
            // Retrieve the password from the PasswordBox.
            string password = pbLecturerPassword.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a full name for the lecturer.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLecturerFullName.Text))
            {
                MessageBox.Show("Please enter a name for the lecturer.");
                return;
            }

            // Get selected subjects.
            var selectedSubjects = lbLecturerSubjects.SelectedItems.Cast<string>().ToList();
            if (!selectedSubjects.Any())
            {
                MessageBox.Show("Please select at least one subject.");
                return;
            }
            // Get selected stages.
            var selectedStages = lbLecturerStages.SelectedItems.Cast<string>().ToList();
            if (!selectedStages.Any())
            {
                MessageBox.Show("Please select at least one stage the lecturer teaches.");
                return;
            }

            var lecturer = new Lecturer
            {
                ID = nextLecturerId,
                UserName = txtLecturerName.Text,
                FullName = txtLecturerFullName.Text,
                Password = password,
                Subjects = selectedSubjects,
                StagesTaught = selectedStages
            };

            lecturers.Add(lecturer);
            SaveLecturers();

            MessageBox.Show($"Lecturer added:\nID: {lecturer.ID}\nName: {lecturer.UserName}");

            nextLecturerId++;
            txtLecturerID.Text = nextLecturerId.ToString();
            txtLecturerName.Clear();
            txtLecturerFullName.Clear();
            pbLecturerPassword.Clear();
            lbLecturerSubjects.UnselectAll();
            lbLecturerStages.UnselectAll();

            // Refresh the Lecturer List grid.
            dgLecturers.Items.Refresh();
        }


        // Add a new group.
        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewGroup.Text))
            {
                MessageBox.Show("Please enter a group name.");
                return;
            }
            if (!Groups.Contains(txtNewGroup.Text))
            {
                Groups.Add(txtNewGroup.Text);
                SaveGroups();
            }
            txtNewGroup.Clear();
        }

        // Add a new stage.
        private void AddStage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewStage.Text))
            {
                MessageBox.Show("Please enter a stage.");
                return;
            }
            if (!Stages.Contains(txtNewStage.Text))
            {
                Stages.Add(txtNewStage.Text);
                SaveStages();
            }
            txtNewStage.Clear();
        }

        // Add a new subject.
        private void AddSubject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewSubject.Text))
            {
                MessageBox.Show("Please enter a subject.");
                return;
            }
            if (!Subjects.Contains(txtNewSubject.Text))
            {
                Subjects.Add(txtNewSubject.Text);
                SaveSubjects();
            }
            txtNewSubject.Clear();
        }

        // Browse for an Excel file.
        private void BrowseExcel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
            if (openFileDialog.ShowDialog() == true)
            {
                txtExcelFilePath.Text = openFileDialog.FileName;
            }
        }

        // Import students from the selected Excel file.
        private void ImportStudents_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExcelFilePath.Text) || !File.Exists(txtExcelFilePath.Text))
            {
                MessageBox.Show("Please select a valid Excel file.");
                return;
            }

            // Ensure that a stage and group are selected.
            if (cbImportStage.SelectedItem == null || cbImportGroup.SelectedItem == null)
            {
                MessageBox.Show("Please select a stage and a group for the imported students.");
                return;
            }

            try
            {
                // Register the code page provider (if not already registered) to support Excel encodings.
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using (var stream = File.Open(txtExcelFilePath.Text, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
                    {
                        // Use the selected stage and group from the import controls.
                        string defaultStage = cbImportStage.SelectedItem.ToString();
                        string defaultGroup = cbImportGroup.SelectedItem.ToString();

                        do
                        {
                            while (reader.Read())
                            {
                                // Assuming the student's name is in the first column (column index 0).
                                var value = reader.GetValue(0);
                                string studentName = value != null ? value.ToString() : string.Empty;
                                if (!string.IsNullOrWhiteSpace(studentName))
                                {
                                    var student = new Student
                                    {
                                        ID = nextStudentId,
                                        Name = studentName,
                                        Stage = defaultStage,
                                        Group = defaultGroup
                                    };
                                    students.Add(student);
                                    nextStudentId++;
                                }
                            }
                        } while (reader.NextResult());
                    }
                }

                SaveStudents();

                // Refresh the DataGrid in the "View Students" tab.
                dgStudents.ItemsSource = null;
                dgStudents.ItemsSource = students;
                txtStudentID.Text = nextStudentId.ToString();

                MessageBox.Show("Students imported successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing Excel file: " + ex.Message);
            }
        }


        private void DeleteAllStudents_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the currently displayed (filtered) students.
            var filteredStudents = dgStudents.ItemsSource as List<Student>;
            if (filteredStudents == null || !filteredStudents.Any())
            {
                MessageBox.Show("No students to delete.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete all the displayed students?",
                                "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Remove each student in the filtered list from the main student list.
                foreach (var student in filteredStudents.ToList()) // use ToList() to avoid modifying collection during iteration
                {
                    students.Remove(student);
                }
                SaveStudents();
                FilterStudents();
                MessageBox.Show("All displayed students have been deleted.");
            }
        }

        // --- New: Filter Students by Stage and Group ---
        private void FilterStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterStudents();
        }

        private void FilterStudents()
        {
            string selectedStage = cbFilterStage.SelectedItem as string;
            string selectedGroup = cbFilterGroup.SelectedItem as string;

            var filteredStudents = students.AsEnumerable();
            if (!string.IsNullOrEmpty(selectedStage))
                filteredStudents = filteredStudents.Where(s => s.Stage == selectedStage);
            if (!string.IsNullOrEmpty(selectedGroup))
                filteredStudents = filteredStudents.Where(s => s.Group == selectedGroup);
            dgStudents.ItemsSource = filteredStudents.ToList();
        }




        private void EditLecturer_Click(object sender, RoutedEventArgs e)
        {
            // Get the lecturer from the Tag property.
            Button btn = sender as Button;
            Lecturer lecturer = btn?.Tag as Lecturer;
            if (lecturer != null)
            {
                // For example, show a modal window (or fill your input fields) for editing.
                // Here, we simply pop up a message.
                MessageBox.Show($"Edit lecturer with ID: {lecturer.ID}");
                // You can create an EditLecturerWindow and pass the lecturer object.
            }
        }

        private void DeleteLecturer_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Lecturer lecturer = btn?.Tag as Lecturer;
            if (lecturer != null)
            {
                if (MessageBox.Show("Are you sure you want to delete this lecturer?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    lecturers.Remove(lecturer);
                    SaveLecturers();
                    dgLecturers.Items.Refresh();
                }
            }
        }




        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            // Get the lecturer from the Tag property.
            Button btn = sender as Button;
            Student student = btn?.Tag as Student;
            if (student != null)
            {
                // For example, show a modal window (or fill your input fields) for editing.
                // Here, we simply pop up a message.
                MessageBox.Show($"Edit student with ID: {student.ID}");
                // You can create an EditLecturerWindow and pass the lecturer object.
            }
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Student student = btn?.Tag as Student;
            if (student != null)
            {
                if (MessageBox.Show("Are you sure you want to delete this student?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    students.Remove(student);
                    SaveStudents();
                    dgStudents.ItemsSource = null;
                    dgStudents.ItemsSource = students;

                    //dgStudents.Items.Refresh();  // Refresh the students grid.
                }
            }
        }

        private void ChangeAdminPassword_Click(object sender, RoutedEventArgs e)
        {
            string currentInput = pbCurrentAdminPassword.Password;
            string newPass = pbNewAdminPassword.Password;
            string confirmPass = pbConfirmAdminPassword.Password;

            // Load the current password from the JSON file.
            string currentStored = LoadAdminPassword();

            if (currentInput != currentStored)
            {
                MessageBox.Show("Current password is incorrect.");
                return;
            }
            if (newPass != confirmPass)
            {
                MessageBox.Show("New passwords do not match.");
                return;
            }

            // Optionally add any additional validations for the new password here.

            // Save the new password to the same JSON file.
            SaveAdminPassword(newPass);
            MessageBox.Show("Admin password changed successfully.");

            // Clear the input fields.
            pbCurrentAdminPassword.Clear();
            pbNewAdminPassword.Clear();
            pbConfirmAdminPassword.Clear();
        }




        public void SaveAdminPassword(string newPassword)
        {
            AdminData adminData = new AdminData { Password = newPassword };
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("admin.json", JsonSerializer.Serialize(adminData, options));
        }


        public string LoadAdminPassword()
        {
            // Check if the file exists
            if (File.Exists("admin.json"))
            {
                string json = File.ReadAllText("admin.json");
                AdminData adminData = JsonSerializer.Deserialize<AdminData>(json);
                // If the file exists and a password is present, return it.
                if (adminData != null && !string.IsNullOrWhiteSpace(adminData.Password))
                {
                    return adminData.Password;
                }
            }
            // Default password if file doesn't exist or is invalid.
            return "admin";
        }




        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string group = btn?.Tag as string;
            if (!string.IsNullOrWhiteSpace(group))
            {
                if (MessageBox.Show($"Are you sure you want to delete the group '{group}'?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Groups.Remove(group);
                    SaveGroups();
                }
            }
        }

        private void DeleteStage_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string stage = btn?.Tag as string;
            if (!string.IsNullOrWhiteSpace(stage))
            {
                if (MessageBox.Show($"Are you sure you want to delete the stage '{stage}'?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Stages.Remove(stage);
                    SaveStages();
                }
            }
        }

        private void DeleteSubject_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string subject = btn?.Tag as string;
            if (!string.IsNullOrWhiteSpace(subject))
            {
                if (MessageBox.Show($"Are you sure you want to delete the subject '{subject}'?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Subjects.Remove(subject);
                    SaveSubjects();
                }
            }
        }


        #endregion
    }



}
