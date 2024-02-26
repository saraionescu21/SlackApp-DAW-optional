using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlackApp.Data;
using SlackApp.Models;

namespace SlackApp.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public MessagesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }
        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            Message mess = db.Messages.Find(id);

            var userId = _userManager.GetUserId(User);

            var IsModerator = await db.Moderators
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == mess.ChannelId);

            if (mess.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin") || IsModerator)
            {
                db.Messages.Remove(mess);
                db.SaveChanges();
                return Redirect("/Channels/Show/" + mess.ChannelId);
            }

            else
            {
                TempData["message"] = "You can't delete the message";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Channels");
            }
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int id)
        {
            Message mess = db.Messages.Find(id);

            if (mess.UserId == _userManager.GetUserId(User))
            {
                return View(mess);
            }

            else
            {
                TempData["message"] = "You can't delete the message";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Channels");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int id, Message requestMessage)
        {
            Message mess = db.Messages.Find(id);

            if (mess.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {
                    mess.Content = requestMessage.Content;

                    db.SaveChanges();

                    return Redirect("/Channels/Show/" + mess.ChannelId);
                }
                else
                {
                    return View(requestMessage);
                }
            }
            else
            {
                TempData["message"] = "You can't modify";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Channels");
            }
        }
    }
}
