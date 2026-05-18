using EducationAssignmentPortal.Data;
using EducationAssignmentPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace EducationAssignmentPortal.Services
{
    public class NotificationService
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        public NotificationService(IDbContextFactory<AppDBContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Notification>> GetNotificationsByRoleAsync(string role)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Notifications
                .Where(n => n.Role == role)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string role)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Notifications
                .CountAsync(n => n.Role == role && !n.IsRead);
        }

        public async Task AddNotificationAsync(string role, string title, string message, string type = "System", string? targetUrl = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var notification = new Notification
            {
                Role = role,
                Title = title,
                Message = message,
                Type = type,
                TargetUrl = targetUrl,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var notification = await context.Notifications.FindAsync(id);

            if (notification != null)
            {
                notification.IsRead = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string role)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var notifications = await context.Notifications
                .Where(n => n.Role == role && !n.IsRead)
                .ToListAsync();

            foreach (var item in notifications)
            {
                item.IsRead = true;
            }

            await context.SaveChangesAsync();
        }
    }
}