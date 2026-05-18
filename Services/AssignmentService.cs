using EducationAssignmentPortal.Models;
using EducationAssignmentPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace EducationAssignmentPortal.Services
{
    public class AssignmentService
    {
        private readonly IDbContextFactory<AppDBContext> _factory;

        public AssignmentService(IDbContextFactory<AppDBContext> factory)
        {
            _factory = factory;
        }

        // 🔹 Get ALL assignments (Admin / Faculty)
        public async Task<List<Assignment>> GetAllAssignments()
        {
            using var context = _factory.CreateDbContext();

            return await context.Assignments
                .Include(a => a.Course)
                .ToListAsync();
        }

        // 🔹 Get assignments by Course
        public async Task<List<Assignment>> GetAssignmentsByCourseAsync(int courseId)
        {
            using var context = _factory.CreateDbContext();

            return await context.Assignments
                .Where(a => a.CourseId == courseId)
                .Include(a => a.Course)
                .ToListAsync();
        }

        // 🔹 Get assignment by Id
        public async Task<Assignment?> GetById(int id)
        {
            using var context = _factory.CreateDbContext();

            return await context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // 🔥 ADD NEW ASSIGNMENT + MAP TO ALL STUDENTS
        public async Task AddAssignmentAsync(Assignment assignment)
        {
            using var context = _factory.CreateDbContext();

            // Step 1: Save assignment
            context.Assignments.Add(assignment);
            await context.SaveChangesAsync();

            // Step 2: Get all students
            var students = await context.Users
                .Where(u => u.Role == "Student")
                .ToListAsync();

            // Step 3: Assign to each student
            foreach (var student in students)
            {
                // Prevent duplicate entry
                bool exists = await context.StudentAssignments.AnyAsync(sa =>
                    sa.StudentId == student.Id &&
                    sa.AssignmentId == assignment.Id);

                if (!exists)
                {
                    context.StudentAssignments.Add(new StudentAssignment
                    {
                        StudentId = student.Id,
                        AssignmentId = assignment.Id,
                        Status = "Pending"
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        // 🔹 Update full assignment
        public async Task UpdateAssignmentAsync(Assignment assignment)
        {
            using var context = _factory.CreateDbContext();

            context.Assignments.Update(assignment);
            await context.SaveChangesAsync();
        }

        // ⚠️ OLD METHOD (DON'T USE FOR STUDENTS)
        public async Task UpdateStatusAsync(int id, string status)
        {
            using var context = _factory.CreateDbContext();

            var assignment = await context.Assignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment != null)
            {
                assignment.Status = status;
                await context.SaveChangesAsync();
            }
        }

        // 🔹 DELETE assignment + related student records
        public async Task DeleteAssignmentAsync(int id)
        {
            using var context = _factory.CreateDbContext();

            var assignment = await context.Assignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment != null)
            {
                // 🔥 remove related student assignments
                var studentAssignments = context.StudentAssignments
                    .Where(sa => sa.AssignmentId == id);

                context.StudentAssignments.RemoveRange(studentAssignments);

                context.Assignments.Remove(assignment);

                await context.SaveChangesAsync();
            }
        }
    }
}