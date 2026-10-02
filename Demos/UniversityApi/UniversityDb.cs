using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

public class UniversityDb : DbContext
{
    public UniversityDb(DbContextOptions<UniversityDb> options) : base(options) { }
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<CourseSection> CourseSections => Set<CourseSection>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Student>().HasIndex(student => student.Email).IsUnique();
        model.Entity<Student>().HasIndex(student => student.RegistrationNumber).IsUnique();
        model.Entity<Teacher>().HasIndex(teacher => teacher.Email).IsUnique();
        model.Entity<Course>().HasIndex(course => course.Code).IsUnique();
        model.Entity<Enrollment>().HasIndex(enrollment =>
            new { enrollment.StudentId, enrollment.CourseId }).IsUnique();
        model.Entity<Course>().HasOne<Teacher>().WithMany()
            .HasForeignKey(course => course.TeacherId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Enrollment>().HasOne<Student>().WithMany()
            .HasForeignKey(enrollment => enrollment.StudentId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Enrollment>().HasOne<Course>().WithMany()
            .HasForeignKey(enrollment => enrollment.CourseId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<CourseSection>().HasIndex(section => new { section.SectionCode, section.Term }).IsUnique();
        model.Entity<CourseSection>().HasOne<Course>().WithMany()
            .HasForeignKey(section => section.CourseId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<CourseSection>().HasOne<Teacher>().WithMany()
            .HasForeignKey(section => section.TeacherId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Enrollment>().HasOne<CourseSection>().WithMany()
            .HasForeignKey(enrollment => enrollment.CourseSectionId).OnDelete(DeleteBehavior.Restrict);
    }
}
