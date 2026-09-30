using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheducate.Data;
using Scheducate.Models;

namespace Scheducate.Controllers
{
    [Authorize]
    public class UserGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserGroupsController(
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /UserGroups
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Groups owned by the current user
            var ownedGroups = await _context.UserGroups
                .Include(g => g.Members)
                .Where(g => g.OwnerId == userId)
                .ToListAsync();

            // Groups where the current user is a member
            var memberGroups = await _context.GroupMembers
                .Include(m => m.UserGroup)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            ViewBag.OwnedGroups = ownedGroups;
            ViewBag.MemberGroups = memberGroups;

            return View();
        }

        // GET: /UserGroups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /UserGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserGroup userGroup)
        {
            // Remove fields that the user should not be responsible for
            ModelState.Remove(nameof(UserGroup.OwnerId));

            if (!ModelState.IsValid)
            {
                return View(userGroup);
            }

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            // Set the owner to the currently logged-in user
            userGroup.OwnerId = userId;

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: /UserGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var userGroup = await _context.UserGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id && g.OwnerId == userId);

            if (userGroup == null)
            {
                return NotFound();
            }

            return View(userGroup);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var userGroup = await _context.UserGroups
                .Include(g => g.Members)
                .Include(g => g.Invitations)
                .FirstOrDefaultAsync(g => g.Id == id && g.OwnerId == userId);

            if (userGroup == null)
            {
                return NotFound();
            }

            // Remove all memberships associated with the group
            _context.GroupMembers.RemoveRange(userGroup.Members);

            // Remove all invitations associated with the group
            _context.GroupInvitations.RemoveRange(userGroup.Invitations);

            // Remove the group
            _context.UserGroups.Remove(userGroup);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: /UserGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var userGroup = await _context.UserGroups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .Include(g => g.Members)
                    .ThenInclude(m => m.SharedSchedule)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (userGroup == null)
            {
                return NotFound();
            }

            var isOwner = userGroup.OwnerId == userId;
            var isMember = userGroup.Members.Any(m => m.UserId == userId);

            if (!isOwner && !isMember)
            {
                return Forbid();
            }

            // Get the current user's schedules
            var schedules = await _context.Schedule
                .Where(s => s.UserId == userId)
                .ToListAsync();

            ViewBag.UserSchedules = schedules;

            // Build overlap schedule
            var sharedSchedules = userGroup.Members
                .Where(m => m.SharedSchedule != null)
                .Select(m => m.SharedSchedule!)
                .ToList();

            byte[] overlapAvailability = new byte[42];

            if (sharedSchedules.Any())
            {
                Array.Copy(
                    sharedSchedules[0].Availability,
                    overlapAvailability,
                    overlapAvailability.Length);

                foreach (var schedule in sharedSchedules.Skip(1))
                {
                    for (int i = 0; i < overlapAvailability.Length; i++)
                    {
                        overlapAvailability[i] &= schedule.Availability[i];
                    }
                }
            }

            ViewBag.OverlapAvailability = overlapAvailability;
            ViewBag.SharedScheduleCount = sharedSchedules.Count;

            return View(userGroup);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSchedule(int groupId, int? scheduleId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var membership = await _context.GroupMembers
                .FirstOrDefaultAsync(m =>
                    m.GroupId == groupId &&
                    m.UserId == userId);

            if (membership == null)
            {
                return Forbid();
            }

            // User selected "Select a schedule"
            if (scheduleId == null)
            {
                membership.SharedScheduleId = null;
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = groupId });
            }

            var scheduleExists = await _context.Schedule
                .AnyAsync(s =>
                    s.Id == scheduleId &&
                    s.UserId == userId);

            if (!scheduleExists)
            {
                return BadRequest();
            }

            membership.SharedScheduleId = scheduleId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = groupId });
        }
        [HttpGet]
        public async Task<IActionResult> Invite(int id)
        {
            var userId = _userManager.GetUserId(User);

            var group = await _context.UserGroups
                .FirstOrDefaultAsync(g => g.Id == id && g.OwnerId == userId);

            if (group == null)
            {
                return NotFound();
            }

            var token = Guid.NewGuid().ToString("N");

            var invitation = new GroupInvitation
            {
                GroupId = group.Id,
                Token = token
            };

            _context.GroupInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            var invitationUrl =
                $"{Request.Scheme}://{Request.Host}/UserGroups/Join/{token}";

            ViewBag.InvitationUrl = invitationUrl;
            ViewBag.GroupName = group.Name;
            ViewBag.GroupId = group.Id;

            return View();
        }
        [AllowAnonymous]
        [HttpGet("/UserGroups/Join/{token}")]
        public async Task<IActionResult> Join(string token)
        {

            if (string.IsNullOrWhiteSpace(token))
            {
                return Content("Token is empty");
            }

            var invitation = await _context.GroupInvitations
                .Include(i => i.UserGroup)
                .FirstOrDefaultAsync(i => i.Token == token);

            if (invitation == null)
            {
                return Content($"Invitation not found for token: {token}");
            }

            if (invitation.UserGroup == null)
            {
                return Content($"Invitation exists, but Group {invitation.GroupId} was not found.");
            }

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                var returnUrl = $"/UserGroups/Join/{Uri.EscapeDataString(token)}";

                return Redirect($"/Identity/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var alreadyMember = await _context.GroupMembers
                .AnyAsync(m =>
                    m.GroupId == invitation.GroupId &&
                    m.UserId == userId);

            if (alreadyMember)
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id = invitation.GroupId });
            }

            return View(invitation);
        }
        // POST: /UserGroups/AcceptInvitation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvitation(string token)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var invitation = await _context.GroupInvitations
                .FirstOrDefaultAsync(i => i.Token == token);

            if (invitation == null)
            {
                return NotFound();
            }

            // Check whether the user is already a member
            var alreadyMember = await _context.GroupMembers
                .AnyAsync(m =>
                    m.GroupId == invitation.GroupId &&
                    m.UserId == userId);

            if (!alreadyMember)
            {
                var groupMember = new GroupMember
                {
                    GroupId = invitation.GroupId,
                    UserId = userId
                };

                _context.GroupMembers.Add(groupMember);
            }

            // Make the invitation single-use
            _context.GroupInvitations.Remove(invitation);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = invitation.GroupId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var group = await _context.UserGroups
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            // Make sure the current user is the owner
            if (group.OwnerId != userId)
            {
                return Forbid();
            }

            // Don't create a duplicate membership
            var alreadyMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == id && m.UserId == userId);

            if (!alreadyMember)
            {
                var membership = new GroupMember
                {
                    GroupId = id,
                    UserId = userId
                };

                _context.GroupMembers.Add(membership);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
