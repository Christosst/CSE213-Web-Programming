using System.Text.Json;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

public class UniversityImport
{
    public List<Student> Students { get; set; } = [];
    public List<ImportTeacher> Teachers { get; set; } = [];
    public List<ImportCourse> Courses { get; set; } = [];
    public List<ImportSection> Sections { get; set; } = [];
    public bool ReplaceDemoSeed { get; set; }
    public string TargetCourseCode { get; set; } = "CSE213";
    public string TargetSectionCode { get; set; } = "46252";
    public string Term { get; set; } = "F2026";
    public DateOnly EnrolledOn { get; set; } = new(2026, 10, 2);
}
public class ImportTeacher : Teacher { public string SourceKey { get; set; } = ""; }
public class ImportCourse : Course { public string? TeacherKey { get; set; } }
public class ImportSection : CourseSection
{
    public string CourseCode { get; set; } = "";
    public string? TeacherKey { get; set; }
}

public static class ImportData
{
    // Run explicitly on a local/private database; HTTP does not expose an import endpoint.
    public static object Import(UniversityDb db, string path)
    {
        var data = JsonSerializer.Deserialize<UniversityImport>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Empty import document");
        if (data.Students.Any(s => string.IsNullOrWhiteSpace(s.RegistrationNumber)))
            throw new InvalidOperationException("Each imported student needs a registration number");
        var targetSection = data.Sections.Single(s => s.SectionCode == data.TargetSectionCode && s.Term == data.Term);
        var targetTeacher = data.Teachers.Single(t => t.SourceKey == targetSection.TeacherKey);
        if (!targetTeacher.FullName.Equals("Christos Stylianides", StringComparison.OrdinalIgnoreCase) || targetSection.CourseCode != "CSE213")
            throw new InvalidOperationException("Target must be Christos Stylianides' CSE213 section");
        using var transaction = db.Database.BeginTransaction();
        var teachers = db.Teachers.ToList();
        var teacherRecords = new Dictionary<string, Teacher>();
        foreach (var source in data.Teachers)
        {
            var item = teachers.FirstOrDefault(t => source.Email != null && t.Email == source.Email)
                ?? teachers.FirstOrDefault(t => t.FullName == source.FullName && t.Email == null);
            if (item is null) { item = new Teacher(); db.Teachers.Add(item); teachers.Add(item); }
            item.FullName = source.FullName; item.Email = source.Email;
            item.WorkdayId = source.WorkdayId; item.EmploymentType = source.EmploymentType;
            teacherRecords[source.SourceKey] = item;
        }
        db.SaveChanges();
        var teacherIds = teacherRecords.ToDictionary(pair => pair.Key, pair => pair.Value.Id);
        var courses = db.Courses.ToList();
        var courseRecords = new Dictionary<string, Course>();
        foreach (var source in data.Courses)
        {
            var item = courses.SingleOrDefault(c => c.Code == source.Code);
            if (item is null) { item = new Course(); db.Courses.Add(item); courses.Add(item); }
            item.Code = source.Code; item.Title = source.Title;
            item.TeacherId = source.TeacherKey is null ? null : teacherIds[source.TeacherKey];
            item.School = source.School; item.Program = source.Program; item.Department = source.Department;
            item.Level = source.Level; item.Ects = source.Ects;
            courseRecords[source.Code] = item;
        }
        db.SaveChanges();
        var courseIds = courseRecords.ToDictionary(pair => pair.Key, pair => pair.Value.Id);
        var sections = db.CourseSections.ToList();
        foreach (var source in data.Sections)
        {
            var item = sections.SingleOrDefault(s => s.SectionCode == source.SectionCode && s.Term == source.Term);
            if (item is null) { item = new CourseSection(); db.CourseSections.Add(item); sections.Add(item); }
            item.SectionCode = source.SectionCode; item.SectionName = source.SectionName; item.Term = source.Term;
            item.CourseId = courseIds[source.CourseCode];
            item.TeacherId = source.TeacherKey is null ? null : teacherIds[source.TeacherKey];
            item.Timing = source.Timing; item.Room = source.Room; item.ModeOfStudy = source.ModeOfStudy;
            item.ReportedEnrolledStudents = source.ReportedEnrolledStudents;
            item.ReportedNonRegisteredStudents = source.ReportedNonRegisteredStudents; item.Capacity = source.Capacity;
            item.School = source.School;
            item.Program = source.Program;
            item.Department = source.Department;
            item.Level = source.Level;
            item.Ects = source.Ects;
            item.TimetableInstructor = source.TimetableInstructor;

        }
        db.SaveChanges();
        var sectionId = sections.Single(s => s.SectionCode == data.TargetSectionCode && s.Term == data.Term).Id;
        var students = db.Students.ToList();
        foreach (var source in data.Students)
        {
            var item = students.SingleOrDefault(s => s.RegistrationNumber == source.RegistrationNumber);
            if (item is null) { item = new Student(); db.Students.Add(item); students.Add(item); }
            item.FullName = source.FullName; item.FirstName = source.FirstName; item.LastName = source.LastName;
            item.Email = source.Email; item.RegistrationNumber = source.RegistrationNumber;
            item.Gender = source.Gender; item.School = source.School; item.Program = source.Program;
            item.Department = source.Department; item.Specialization = source.Specialization;
            item.AnnualResultsModel = source.AnnualResultsModel; item.Curriculum = source.Curriculum;
            item.ApplicationId = source.ApplicationId; item.CumulativeGpa = source.CumulativeGpa;
            db.SaveChanges();
            var courseId = courseIds[data.TargetCourseCode];
            var enrollment = db.Enrollments.SingleOrDefault(e => e.StudentId == item.Id && e.CourseId == courseId);
            if (enrollment is null) db.Enrollments.Add(new Enrollment { StudentId = item.Id,
                CourseId = courseId, CourseSectionId = sectionId, EnrolledOn = data.EnrolledOn });
            else enrollment.CourseSectionId = sectionId;
        }
        db.SaveChanges();
        if (data.ReplaceDemoSeed)
        {
            // Remove only the identifiable, untouched original demo records.
            var demoStudents = db.Students.Where(s => s.RegistrationNumber == null &&
                s.FullName == "Alex Demo" && s.Email == "student@example.org").ToList();
            foreach (var demo in demoStudents)
            {
                db.Enrollments.RemoveRange(db.Enrollments.Where(e => e.StudentId == demo.Id));
                db.Students.Remove(demo);
            }
            db.SaveChanges();
            var demoSections = db.CourseSections.Where(s => s.SectionCode == "DEMO-ECA" &&
                s.SectionName == "CSE213 - ECA (demo)").ToList();
            foreach (var demo in demoSections)
                if (!db.Enrollments.Any(e => e.CourseSectionId == demo.Id)) db.CourseSections.Remove(demo);
            db.SaveChanges();
            var demoTeachers = db.Teachers.Where(t => t.Email == "teacher@example.org" &&
                (t.FullName == "Dr Demo Teacher" || t.FullName == "Christos Stylianides")).ToList();
            foreach (var demo in demoTeachers)
                if (!db.Courses.Any(c => c.TeacherId == demo.Id) && !db.CourseSections.Any(s => s.TeacherId == demo.Id))
                    db.Teachers.Remove(demo);
            db.SaveChanges();
        }
        transaction.Commit();
        return new { ImportedStudents = data.Students.Count, ImportedTeachers = data.Teachers.Count,
            ImportedCourses = data.Courses.Count, ImportedSections = data.Sections.Count,
            EnrolledStudents = data.Students.Count, data.TargetCourseCode, data.TargetSectionCode, data.Term };
    }
}
