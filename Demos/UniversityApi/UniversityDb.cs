using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

public class UniversityDb : DbContext
{
    public UniversityDb(DbContextOptions<UniversityDb> options) : base(options) { }
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Student>().HasIndex(student => student.Email).IsUnique();
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
    }
}
