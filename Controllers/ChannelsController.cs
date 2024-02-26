using SlackApp.Controllers;
using SlackApp.Data;
using SlackApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using System.Text.RegularExpressions;

namespace SlackApp.Controllers
{
    public class ChannelsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ChannelsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index()
        {

            int _perPage = 3;

            var channels = db.Channels.Include("Category").Include("User").OrderBy(c => c.Category.CategoryName);

            var search = "";

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {

                
                search =
                Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> channelIds = db.Channels.Where
                (
                at => at.Name.Contains(search)
                //|| at.Description.Contains(search)
                ).Select(a => a.Id).ToList();

               
                channels = db.Channels.Where(chn =>
                channelIds.Contains(chn.Id))
                .Include("Category")
                .Include("User")
                .OrderBy(c => c.Category.CategoryName);
            }

            ViewBag.SearchString = search;

            int totalItems = channels.Count();
            var currentPage =
            Convert.ToInt32(HttpContext.Request.Query["page"]);
            
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedChannels =
                channels.Skip(offset).Take(_perPage);

            
            ViewBag.lastPage = Math.Ceiling((float)totalItems /
            (float)_perPage);
            
            ViewBag.Channels = paginatedChannels;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Channels/Index/?search="
                + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Channels/Index/?page";
            }

            return View();
        }



        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index2(int id)
        {
            Request request = new Request
            {
                ChannelId = id,
                UserId = _userManager.GetUserId(User)
            };
            db.Requests.Add(request);
            db.SaveChanges();

            TempData["message"] = "Your request has been sent successfully!";
            TempData["messageType"] = "alert-success";

            return Redirect("/Channels/Index/");
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index1()
        {
            var userChannels = db.UserChannels
            .Where(uc => uc.UserId == _userManager.GetUserId(User))
            .Select(uc => uc.ChannelId)
            .Distinct()
            .ToList();

            var channels = db.Channels
                .Where(c => userChannels.Contains(c.Id))
                .Include("Category")
                .Include("User")
                .OrderBy(c => c.Category.CategoryName);
            
            ViewBag.Channels = channels;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Show(int id)
        {

            SetAccessRights(id);

            var userId = _userManager.GetUserId(User);

            var userChannelExists = await db.UserChannels
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            if (userChannelExists || User.IsInRole("Admin"))
            {
                Channel channel = db.Channels.Include("Category")
                                             .Include("User")
                                             .Include("Messages")
                                             .Include("Messages.User")
                                             .Where(chn => chn.Id == id)
                                             .First();


                if (channel != null)
                {
                    return View(channel);
                }
                else {
                    return NotFound();
                }
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show([FromForm] Message message)
        {
            message.Date = DateTime.Now;
            message.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();
                return Redirect("/Channels/Show/" + message.ChannelId);
            }

            else
            {
                Channel chn = db.Channels.Include("Category")
                                         .Include("User")
                                         .Include("Messages")
                                         .Include("Messages.User")
                                         .Where(chnn => chnn.Id == message.ChannelId)
                                         .First();


                SetAccessRights(message.ChannelId);

                return View(chn);
            }
        }
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> UserList(int id)
        {
            SetAccessRights(id);

            var userId = _userManager.GetUserId(User);

            var userChannelExists = await db.UserChannels
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            if (userChannelExists || User.IsInRole("Admin"))
            {
                var userChannels = db.UserChannels
                .Where(uc => uc.ChannelId == id)
                .Select(uc => uc.UserId)
                .Distinct()
                .ToList();

                var users = db.Users
                    .Where(c => userChannels.Contains(c.Id));

                var moderators = db.Moderators
                .Where(uc => uc.ChannelId == id)
                .Select(uc => uc.UserId)
                .Distinct()
                .ToList();

                var mods = db.Users
                    .Where(c => moderators.Contains(c.Id));

                var usersWithoutModerators = users.Except(mods);

                Channel chn = db.Channels
                                         .Where(chnn => chnn.Id == id)
                                         .First();

                var owner = db.Users.Where(u => u.Id == chn.UserId);
                
                var modswithoutowner = mods.Except(owner);

                var owner2 = db.Users.Where(u => u.Id == chn.UserId).First();

                ViewBag.Owner = owner2;
                ViewBag.Moderators = modswithoutowner;
                ViewBag.UsersList = usersWithoutModerators;
                ViewBag.ChannelId = id;

                return View();
            }
            return Forbid();
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult New()
        {
            Channel channel = new Channel();

            channel.Categ = GetAllCategories();


            return View(channel);
        }


        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public IActionResult New(Channel channel)
        {
            channel.Date = DateTime.Now;

            channel.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Channels.Add(channel);
                db.SaveChanges();
                SubscribedChannel userChannel = new SubscribedChannel
                {
                    ChannelId = channel.Id,
                    UserId = _userManager.GetUserId(User)
                };
                Moderator modrt = new Moderator
                {
                    ChannelId = channel.Id,
                    UserId = _userManager.GetUserId(User)
                };
                db.UserChannels.Add(userChannel);
                db.Moderators.Add(modrt);
                db.SaveChanges();
                TempData["message"] = "Channel created";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                channel.Categ = GetAllCategories();
                return View(channel);
            }
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Edit(int id)
        {

            Channel channel = db.Channels.Include("Category")
                                        .Where(chn => chn.Id == id)
                                        .First();

            channel.Categ = GetAllCategories();

            var userId = _userManager.GetUserId(User);

            var IsModerator = await db.Moderators
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            if (channel.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin") || IsModerator)
            {
                return View(channel);
            }

            else
            {
                TempData["message"] = "You do not have the right to make modifications to a channel that does not belong to you";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Edit(int id, Channel requestChannel)
        {
            Channel channel = db.Channels.Find(id);

            var userId = _userManager.GetUserId(User);

            var IsModerator = await db.Moderators
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            if (ModelState.IsValid)
            {
                if (channel.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin") || IsModerator)
                {
                    channel.Name = requestChannel.Name;
                    channel.Description = requestChannel.Description;
                    channel.CategoryId = requestChannel.CategoryId;
                    TempData["message"] = "Channel modified";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You can't modify this channel";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestChannel.Categ = GetAllCategories();
                return View(requestChannel);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public ActionResult Delete(int id)
        {
            Channel channel = db.Channels.Include("Messages")
                                         .Where(chn => chn.Id == id)
                                         .First();

            if (channel.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Messages.RemoveRange(channel.Messages);
                db.Channels.Remove(channel);
                db.SaveChanges();
                TempData["message"] = "Channel deleted";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "You can't delete this channel";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> RequestList(int id)
        {
            SetAccessRights(id);

            var userId = _userManager.GetUserId(User);

            var userChannelExists = await db.UserChannels
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            var IsModerator = await db.Moderators
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            if ((userChannelExists && IsModerator) || User.IsInRole("Admin"))
            {
                var requests = db.Requests
                .Where(uc => uc.ChannelId == id)
                .Select(uc => uc.UserId)
                .Distinct()
                .ToList();

                var users = db.Users
                    .Where(c => requests.Contains(c.Id));

                ViewBag.UsersList = users;
                ViewBag.ChannelId = id;

                return View();
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> RequestList(int id, string id2)
        {

            var userId = _userManager.GetUserId(User);

            var userChannelExists = await db.UserChannels
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            var IsModerator = await db.Moderators
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);


            if ((userChannelExists && IsModerator) || User.IsInRole("Admin"))
            {
                SubscribedChannel userChannel = new SubscribedChannel
                {
                    ChannelId = id,
                    UserId = id2
                };
                db.UserChannels.Add(userChannel);
                Request rq = db.Requests
                                      .Where(r => r.UserId == id2 && r.ChannelId == id)
                                      .First();
                db.Requests.Remove(rq);
                db.SaveChanges();

                return Redirect("/Channels/RequestList/" + id);
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Kick(int id, string id2)
        {

            var userId = _userManager.GetUserId(User);

            var userChannelExists = await db.UserChannels
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            var IsModerator = await db.Moderators
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            Channel chn = db.Channels
                                         .Where(chnn => chnn.Id == id)
                                         .First();

            var owner = db.Users.Where(u => u.Id == chn.UserId).First();

            if ((userChannelExists && IsModerator && id2 != owner.Id) || User.IsInRole("Admin"))
            {
                SubscribedChannel uc = db.UserChannels
                                      .Where(uc => uc.UserId == id2 && uc.ChannelId == id)
                                      .First();
                
                db.UserChannels.Remove(uc);

                var IsModerator2 = await db.Moderators
                .AnyAsync(uc => uc.UserId == id2 && uc.ChannelId == id);
                if (IsModerator2)
                {
                    Moderator m = db.Moderators
                                          .Where(m => m.UserId == id2 && m.ChannelId == id)
                                          .First();
                    db.Moderators.Remove(m);
                }
                db.SaveChanges();

                return Redirect("/Channels/UserList/" + id);
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Promote(int id, string id2)
        {

            var userId = _userManager.GetUserId(User);

            var userChannelExists = await db.UserChannels
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            var IsModerator = await db.Moderators
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            Channel chn = db.Channels
                                         .Where(chnn => chnn.Id == id)
                                         .First();

            var owner = db.Users.Where(u => u.Id == chn.UserId).First();

            if ((userChannelExists && IsModerator && id2 != owner.Id) || User.IsInRole("Admin"))
            {
                Moderator moderator = new Moderator
                {
                    ChannelId = id,
                    UserId = id2
                };
                db.Moderators.Add(moderator);
                db.SaveChanges();

                return Redirect("/Channels/UserList/" + id);
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public async Task<IActionResult> Demote(int id, string id2)
        {

            var userId = _userManager.GetUserId(User);

            var userChannelExists = await db.UserChannels
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            var IsModerator = await db.Moderators
                .AnyAsync(uc => uc.UserId == userId && uc.ChannelId == id);

            Channel chn = db.Channels
                                         .Where(chnn => chnn.Id == id)
                                         .First();

            var owner = db.Users.Where(u => u.Id == chn.UserId).First();

            if ((userChannelExists && IsModerator && id2 != owner.Id) || User.IsInRole("Admin"))
            {
                var IsModerator2 = await db.Moderators
                .AnyAsync(uc => uc.UserId == id2 && uc.ChannelId == id);
                if (IsModerator2)
                {
                    Moderator m = db.Moderators
                                          .Where(m => m.UserId == id2 && m.ChannelId == id)
                                          .First();
                    db.Moderators.Remove(m);
                }
                db.SaveChanges();

                return Redirect("/Channels/UserList/" + id);
            }
            return Forbid();
        }


        private void SetAccessRights(int? id)
        {

            ViewBag.AfisareButoane = false;
            
            var userId = _userManager.GetUserId(User);

            bool IsModerator = db.Moderators.Any(m =>
            m.UserId == userId && m.ChannelId == id);

            if (User.IsInRole("Moderator") || User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteModerator = IsModerator;
            ViewBag.EsteAdmin = User.IsInRole("Admin");
            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            
            var selectList = new List<SelectListItem>();

            
            var categories = from cat in db.Categories
                             select cat;

            
            foreach (var category in categories)
            {
                
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            return selectList;
        }
    }
}
