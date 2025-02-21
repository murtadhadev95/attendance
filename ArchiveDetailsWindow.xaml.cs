using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AttendanceApp
{
    public partial class ArchiveDetailsWindow : Window
    {
        // This property is used to display the header in the UI.
        public string ArchiveHeader { get; set; }

        // Store the original record for use in export.
        private AttendanceRecord _record;
        private string _lecturerFullName;

        // Lecturer name, passed from the calling context.

        // Updated constructor now accepts the lecturer's full name.
        public ArchiveDetailsWindow(AttendanceRecord record, string lecturerFullName)
        {
            InitializeComponent();
            _record = record;
            _lecturerFullName = lecturerFullName;
            ArchiveHeader = $"Lecturer: {_lecturerFullName}\nDate: {record.Date.ToShortDateString()}  Stage: {record.Stage}  Subject: {record.Subject}";
            DataContext = this;

            // Build group details for UI display.
            List<GroupAttendanceDetail> details = new List<GroupAttendanceDetail>();
            foreach (var kvp in record.GroupAttendance)
            {
                details.Add(new GroupAttendanceDetail
                {
                    GroupName = kvp.Key,
                    Count = kvp.Value.Count,
                    // Each absent name on a new line.
                    AbsentNames = string.Join("\n", kvp.Value)
                });
            }
            icGroupDetails.ItemsSource = details;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportAttendance_Click(object sender, RoutedEventArgs e)
        {
            // Create a new Excel workbook using ClosedXML.
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Absent Students");

            // 1. Write header row (Row 1)
            // Column A: Lecturer Name, Column B: Date, Column C: Subject, Column D: Stage.
            worksheet.Cell(1, 1).Value = _lecturerFullName;
            worksheet.Cell(1, 2).Value = _record.Date.ToString("yyyy-MM-dd");
            worksheet.Cell(1, 3).Value = _record.Subject;
            worksheet.Cell(1, 4).Value = _record.Stage;

            // 2. Write group data using the actual groups in _record.GroupAttendance.
            int col = 1;
            foreach (var kvp in _record.GroupAttendance)
            {
                // Write group header (e.g., "A") in Row 2.
                worksheet.Cell(2, col).Value = kvp.Key;
                int row = 3;
                foreach (var name in kvp.Value)
                {
                    worksheet.Cell(row, col).Value = name;
                    row++;
                }
                col++;
            }

            // Adjust columns to fit contents.
            worksheet.Columns().AdjustToContents();

            // 3. Build the file name using date, stage, and subject.
            string fileName = $"{_record.Date:yyyy-MM-dd}_{_record.Stage}_{_record.Subject}.xlsx";

            // Use SaveFileDialog so the user can choose where to save.
            var dialog = new SaveFileDialog
            {
                Filter = "Excel Workbook|*.xlsx",
                Title = "Save Absent Students",
                FileName = fileName
            };

            if (dialog.ShowDialog() == true)
            {
                workbook.SaveAs(dialog.FileName);
                MessageBox.Show("Export successful!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class GroupAttendanceDetail
    {
        public string GroupName { get; set; }
        public int Count { get; set; }
        public string AbsentNames { get; set; }
    }
}
