using System.Collections.Generic;

namespace AttendanceApp
{
    public class Lecturer
    {
        public int ID { get; set; }
        // Instead of just "Name", add a full name property.
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public List<string> Subjects { get; set; }
        public List<string> StagesTaught { get; set; }

        public string SubjectsDisplay => Subjects != null ? string.Join(", ", Subjects) : string.Empty;
        public string StagesTaughtDisplay => StagesTaught != null ? string.Join(", ", StagesTaught) : string.Empty;
    }


}
