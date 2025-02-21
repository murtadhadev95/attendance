using System.Collections.Generic;
using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AttendanceApp
{
    public class Attendance
    {
        public DateTime Date { get; set; }
        public string Stage { get; set; }
        public string Subject { get; set; }
        // Dictionary where key is group identifier (e.g., "A") and value is list of absent student names.
        public Dictionary<string, List<string>> GroupAttendance { get; set; } = new Dictionary<string, List<string>>();
    }
}
