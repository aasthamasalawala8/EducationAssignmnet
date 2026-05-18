using EducationAssignmentPortal.Data;
using EducationAssignmentPortal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EducationAssignmentPortal.Services
{
    public class UserService
    {
        private readonly AppDBContext _context;

        public User? LoggedInUser { get; set; }

        public UserService(AppDBContext context)
        {
            _context = context;
        }

        public async Task Register(User user)
        {
            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (user.Role == "Student")
            {
                var assignments = await _context.Assignments.ToListAsync();

                foreach (var assignment in assignments)
                {
                    bool exists = await _context.StudentAssignments.AnyAsync(sa =>
                        sa.StudentId == user.Id &&
                        sa.AssignmentId == assignment.Id);

                    if (!exists)
                    {
                        _context.StudentAssignments.Add(new StudentAssignment
                        {
                            StudentId = user.Id,
                            AssignmentId = assignment.Id,
                            Status = "Pending"
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> ValidateUser(string? email, string? password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
                return null;

            var passwordHasher = new PasswordHasher<User>();

            var result = passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                password
            );

            if (result == PasswordVerificationResult.Success)
            {
                LoggedInUser = user;
                return user;
            }

            return null;
        }

        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public void ApproveFaculty(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsApproved = true;
                _context.SaveChanges();
            }
        }

        public void DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public User? GetCurrentUser()
        {
            return LoggedInUser;
        }
    }
}