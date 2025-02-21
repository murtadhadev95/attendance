using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceApp
{
    public partial class LecturerMainPortal : Page
    {
        private Lecturer currentLecturer;
        private List<Student> allStudents = new List<Student>();
        private List<StudentAttendance> studentAttendances = new List<StudentAttendance>();

        public LecturerMainPortal(Lecturer lecturer)
        {
            InitializeComponent();
            currentLecturer = lecturer;
            InitializeDropDowns();
            LoadGlobalData();
            LoadArchivedRecords();
        }

        private void InitializeDropDowns()
        {
            // Populate Stage and Subject drop-downs from the lecturer's record.
            cbStage.ItemsSource = currentLecturer.StagesTaught;
            cbSubject.ItemsSource = currentLecturer.Subjects;
            // Load groups from global admin data.
            if (File.Exists("groups.json"))
            {
                var json = File.ReadAllText("groups.json");
                var groups = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                cbGroup.ItemsSource = groups;
            }
        }

        private void LoadGlobalData()
        {
            // Load all students from global storage.
            if (File.Exists("students.json"))
            {
                var json = File.ReadAllText("students.json");
                allStudents = JsonSerializer.Deserialize<List<Student>>(json) ?? new List<Student>();
            }
        }

        // Refresh the student list when Stage or Group changes.
        private void cbStageOrGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStudentsForAttendance();
        }

        private void LoadStudentsForAttendance()
        {
            if (cbStage.SelectedItem == null || cbGroup.SelectedItem == null)
            {
                lvStudents.ItemsSource = null;
                return;
            }
            string selectedStage = cbStage.SelectedItem.ToString();
            string selectedGroup = cbGroup.SelectedItem.ToString();

            // Filter students by the selected stage and group.
            var filtered = allStudents.Where(s => s.Stage == selectedStage && s.Group == selectedGroup).ToList();

            // Create an attendance list with all students marked as present.
            studentAttendances = filtered.Select(s => new StudentAttendance
            {
                StudentId = s.ID,
                Name = s.Name,
                IsPresent = true
            }).ToList();
            lvStudents.ItemsSource = studentAttendances;
        }

        // Save attendance (only absent students) into archive.
        private void SaveAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (cbStage.SelectedItem == null || cbSubject.SelectedItem == null || cbGroup.SelectedItem == null)
            {
                MessageBox.Show("Please select Stage, Subject, and Group.");
                return;
            }
            string selectedStage = cbStage.SelectedItem.ToString();
            string selectedSubject = cbSubject.SelectedItem.ToString();
            string selectedGroup = cbGroup.SelectedItem.ToString();

            // Collect absent students (unchecked boxes).
            var absentStudents = studentAttendances.Where(sa => !sa.IsPresent).Select(sa => sa.Name).ToList();
            if (!absentStudents.Any())
            {
                MessageBox.Show("No absent students recorded.");
                return;
            }

            // Create an attendance record and assign the current lecturer's ID.
            AttendanceRecord record = new AttendanceRecord
            {
                Date = DateTime.Today,
                Stage = selectedStage,
                Subject = selectedSubject,
                Group = selectedGroup,
                AbsentStudents = absentStudents,
                LecturerId = currentLecturer.ID
            };

            // Archive attendance using "attendance_archive.json".
            List<AttendanceRecord> records = new List<AttendanceRecord>();
            if (File.Exists("attendance_archive.json"))
            {
                var json = File.ReadAllText("attendance_archive.json");
                records = JsonSerializer.Deserialize<List<AttendanceRecord>>(json) ?? new List<AttendanceRecord>();
            }
            // Check if a record exists for today, stage, subject, and lecturer.
            var existing = records.FirstOrDefault(r =>
                 r.Date == DateTime.Today &&
                 r.Stage == selectedStage &&
                 r.Subject == selectedSubject &&
                 r.LecturerId == currentLecturer.ID);
            if (existing != null)
            {
                // Merge group attendance.
                existing.GroupAttendance[selectedGroup] = absentStudents;
            }
            else
            {
                record.GroupAttendance = new Dictionary<string, List<string>>
                {
                    { selectedGroup, absentStudents }
                };
                records.Add(record);
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("attendance_archive.json", JsonSerializer.Serialize(records, options));
            MessageBox.Show("Attendance saved successfully.");

            // Refresh archived records.
            LoadArchivedRecords();
        }

        // Placeholder for exporting attendance to Excel.
        private void ExportAttendance_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to Excel functionality is not implemented in this example.");
        }

        // Back button returns to the Lecturer Login (or main dashboard if preferred).
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainContainer.AppFrame.Navigate(new MainDashboard());
        }

        // Load archived attendance records for the current lecturer.
        private void LoadArchivedRecords()
        {
            List<AttendanceRecord> records = new List<AttendanceRecord>();
            if (File.Exists("attendance_archive.json"))
            {
                var json = File.ReadAllText("attendance_archive.json");
                records = JsonSerializer.Deserialize<List<AttendanceRecord>>(json) ?? new List<AttendanceRecord>();
            }
            // Filter records to only those for the current lecturer.
            var filteredRecords = records.Where(r => r.LecturerId == currentLecturer.ID).ToList();
            lbArchived.ItemsSource = filteredRecords;
        }

        // When an archive record is selected, open the Archive Details window.
        private void lbArchived_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbArchived.SelectedItem is AttendanceRecord record)
            {
                // Pass the record + lecturer's FullName to ArchiveDetailsWindow:
                var detailsWindow = new ArchiveDetailsWindow(record, currentLecturer.FullName);
                detailsWindow.ShowDialog();
            }
        }
    }

    // A helper class for attendance tracking.
    public class StudentAttendance
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public bool IsPresent { get; set; }
    }

    // Attendance record model with LecturerId and a computed summary.
    public class AttendanceRecord
    {
        public DateTime Date { get; set; }
        public string Stage { get; set; }
        public string Subject { get; set; }
        public string Group { get; set; }
        public List<string> AbsentStudents { get; set; }
        public Dictionary<string, List<string>> GroupAttendance { get; set; } = new Dictionary<string, List<string>>();
        public int LecturerId { get; set; }  // To restrict archive per lecturer

        // Computed property to display a summary in the archive list.
        public string Summary
        {
            get
            {
                string groups = (GroupAttendance != null && GroupAttendance.Any())
                    ? string.Join(", ", GroupAttendance.Keys)
                    : Group;
                return $"{Date.ToShortDateString()} - {Stage} - {Subject} (Groups: {groups})";
            }
        }
    }
}
