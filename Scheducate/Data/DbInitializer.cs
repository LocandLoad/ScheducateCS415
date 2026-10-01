using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scheducate.Models;

namespace Scheducate.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            // 1. Create demo users

            var student = await CreateUserIfNotExists(
                userManager,
                "demo.student@example.com",
                "DemoStudent123!"
            );

            var friend = await CreateUserIfNotExists(
                userManager,
                "demo.friend@example.com",
                "DemoFriend123!"
            );


            // 2. Create demo schedules

            var studentSchedule = await GetOrCreateSchedule(
                context,
                student,
                "Demo Student Schedule",
                new[]
                {
                    (day: 0, startSlot: 18, endSlot: 24), // Sunday 9 AM–12 PM
                    (day: 2, startSlot: 20, endSlot: 26),  // Tuesday 10 AM–1 PM
                    (day: 3, startSlot: 18, endSlot: 24)  // Wednesday 9 AM–12 PM
                }
            );
            

            var friendSchedule = await GetOrCreateSchedule(
                context,
                friend,
                "Demo Friend Schedule",
                new[]
                {
                    (day: 0, startSlot: 22, endSlot: 32), // Sunday 11 AM–4 PM
                    (day: 3, startSlot: 18, endSlot: 24)  // Wednsday 9 AM–12 PM
                }
            );


            // 3. Create demo group

            var group = await context.UserGroups
                .FirstOrDefaultAsync(g =>
                    g.Name == "Computer Science Group" &&
                    g.OwnerId == student.Id);

            if (group == null)
            {
                group = new UserGroup
                {
                    Name = "Computer Science Group",
                    OwnerId = student.Id
                };

                context.UserGroups.Add(group);
                await context.SaveChangesAsync();
            }


            // 4. Add student to group

            var studentMembership =
                await context.GroupMembers.FirstOrDefaultAsync(m =>
                    m.GroupId == group.Id &&
                    m.UserId == student.Id);

            if (studentMembership == null)
            {
                context.GroupMembers.Add(new GroupMember
                {
                    UserId = student.Id,
                    GroupId = group.Id,
                    SharedScheduleId = studentSchedule.Id
                });
            }


            // 5. Add friend to group

            var friendMembership =
                await context.GroupMembers.FirstOrDefaultAsync(m =>
                    m.GroupId == group.Id &&
                    m.UserId == friend.Id);

            if (friendMembership == null)
            {
                context.GroupMembers.Add(new GroupMember
                {
                    UserId = friend.Id,
                    GroupId = group.Id,
                    SharedScheduleId = friendSchedule.Id
                });
            }

            await context.SaveChangesAsync();
        }


        private static async Task<User> CreateUserIfNotExists(
            UserManager<User> userManager,
            string email,
            string password)
        {
            var existingUser =
                await userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return existingUser;
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result =
                await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description)
                );

                throw new Exception(
                    $"Failed to create demo user {email}: {errors}"
                );
            }

            return user;
        }


        private static async Task<Schedule> GetOrCreateSchedule(
            ApplicationDbContext context,
            User user,
            string scheduleName,
            (int day, int startSlot, int endSlot)[] availability)
        {
            var schedule = await context.Schedule
                .FirstOrDefaultAsync(s =>
                    s.UserId == user.Id &&
                    s.Name == scheduleName);

            if (schedule != null)
            {
                return schedule;
            }

            schedule = new Schedule
            {
                Name = scheduleName,
                UserId = user.Id
            };

            foreach (var period in availability)
            {
                SetAvailability(
                    schedule.Availability,
                    period.day,
                    period.startSlot,
                    period.endSlot);
            }

            context.Schedule.Add(schedule);
            await context.SaveChangesAsync();

            return schedule;
        }

        private static void SetAvailability(
            byte[] availability,
            int day,
            int startSlot,
            int endSlot)
        {
            for (int slot = startSlot; slot < endSlot; slot++)
            {
                int bitIndex = day * 48 + slot;

                int byteIndex = bitIndex / 8;
                int bitOffset = bitIndex % 8;

                availability[byteIndex] |=
                    (byte)(1 << bitOffset);
            }
        }
    }
}