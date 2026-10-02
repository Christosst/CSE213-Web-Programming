namespace UniversityApi;

public static class DemoData
{
    public static void Seed(UniversityDb db)
    {
        var teacher = new Teacher { FullName = "Dr Demo Teacher", Email = "teacher@example.org" };
        var student = new Student { FullName = "Alex Demo", Email = "student@example.org" };
        db.Teachers.Add(teacher);
        db.Students.Add(student);
        db.SaveChanges();
        var course = new Course { Code = "CSE213", Title = "Web Programming", TeacherId = teacher.Id };
        db.Courses.Add(course);
        db.SaveChanges();
        db.Enrollments.Add(new Enrollment { StudentId = student.Id, CourseId = course.Id,
            EnrolledOn = new DateOnly(2026, 10, 1) });
        db.SaveChanges();
    }
}
