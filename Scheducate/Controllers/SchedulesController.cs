using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheducate.Data;
using Scheducate.Models;

namespace Scheducate.Controllers
{
    [Authorize]
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public SchedulesController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var schedules = await _context.Schedule
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return View(schedules);
        }

        public IActionResult Create()
        {
            return View(new Schedule
            {
                Name = ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Availability")] Schedule schedule)
        {
            schedule.UserId = _userManager.GetUserId(User)!;

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    foreach (var message in error.Value.Errors)
                    {
                        Console.WriteLine($"{error.Key}: {message.ErrorMessage}");
                    }
                }

                return View(schedule);
            }

            if (!ModelState.IsValid)
            {
                return View(schedule);
            }

            _context.Schedule.Add(schedule);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var schedule = await _context.Schedule
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var schedule = await _context.Schedule
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (schedule == null)
            {
                return NotFound();
            }

            // Remove this schedule from any groups where it is being shared
            var groupMembers = await _context.GroupMembers
                .Where(m => m.SharedScheduleId == id)
                .ToListAsync();

            foreach (var member in groupMembers)
            {
                member.SharedScheduleId = null;
            }

            _context.Schedule.Remove(schedule);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
