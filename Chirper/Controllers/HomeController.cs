using System.Collections.Generic;
using JavaGeneration.Chirper.ViewModels;

namespace JavaGeneration.Chirper.Controllers
{
    using System.Web.Mvc;
    using Models;

    [HandleError]
    public class HomeController : Controller
    {
        public HomeController()
            : this(new Repository())
        {
        }

        private HomeController(IRepository repository)
        {
            Repository = repository;
        }

        private IRepository Repository { get; set; }

        #region Handle HTTP Requests (GET and POST)

        public ActionResult Index()
        {
            return !User.Identity.IsAuthenticated ? PublicTimeLine() : TimeLine();
        }

        [Authorize]
        public ActionResult TimeLine()
        {

            ViewData["Title"] = string.Format("{0} Timeline", User.Identity.Name);
            var aTimeLineData = CreateTimeLineData(User.Identity.Name);
            return View("Index", aTimeLineData);
        }

        public ActionResult UserLine(string id)
        {
            User user;
            var result = GetUser(id, out user);
            if (result != null)
            {
                return result;
            }

            ViewData["Title"] = string.Format("{0} Userline", user.Name);
            ViewData["User"] = user;
            IList<Tweet> userLine = Repository.GetUserLine(user.Name);

            // TODO: populate following count
            // TODO: populate followers count
            var timeLineData = new TimeLineData {Chirps = userLine};
            return View("Index", timeLineData);
        }

        public ActionResult PublicTimeLine()
        {
            ViewData["Title"] = "Public Timeline";
            var timeLine = new TimeLineData {Chirps = Repository.GetPublicTimeLine()};
            return View("Index", timeLine);
        }

        public ActionResult ShowTweet(string id)
        {
            var tweet = Repository.GetTweet(id);
            if (tweet == null)
            {
                return View("Error", new ErrorInfo("Unable to load tweet.", null));
            }

            var timelinedata = new TimeLineData
                                   {
                                       Chirps = new List<Tweet> {tweet},
                                       ShowIndividualChrip = true
                                   };
            ViewData["Title"] = "Show Tweet";
            User user = Repository.GetUser(tweet.User);
            ViewData["User"] = user;
            return View("Index", timelinedata);
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Tweet(string text)
        {
            Repository.AddTweet(new Tweet
                                    {
                                        Location = "Web",
                                        Text = text,
                                        User = User.Identity.Name
                                    });

            return TimeLine();
        }

        [Authorize]
        public ActionResult Follow(string id)
        {
            if (!Repository.Follow(User.Identity.Name, id))
            {
                return View("Error", new ErrorInfo(string.Format("Unable to follow {0}", id)));
            }

            return RedirectToAction("Following");
        }

        public ActionResult Followers(string id)
        {
            User user;
            return GetUser(id, out user) ?? View(Repository.GetFollowers(user.Name));
        }

        public ActionResult Following(string id)
        {
            User user;
            return GetUser(id, out user) ?? View(Repository.GetFollowing(user.Name));
        }

        public ActionResult About()
        {
            return View();
        }

        #endregion

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewData["LoggedUser"] = Repository.GetUser(User.Identity.Name);
            }
        }

        private ActionResult GetUser(string id, out User user)
        {
            if (string.IsNullOrEmpty(id))
            {
                if (User.Identity.IsAuthenticated)
                {
                    id = User.Identity.Name;
                }
                else
                {
                    user = null;
                    return RedirectToAction("LogOn", "Account");
                }
            }

            user = Repository.GetUser(id);
            return user == null
                       ? View("Error", new ErrorInfo(string.Format("Unknown user {0}", id)))
                       : null;
        }

        public ContentResult CurrentUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Content(User.Identity.Name);

            }
            return Content("Unkown");
        }

        private TimeLineData CreateTimeLineData(string userName)
        {
            IList<Tweet> timeLine = Repository.GetTimeLine(userName);
            int followersCount = Repository.GetFollowers(userName).Count;
            int followingCount = Repository.GetFollowing(userName).Count;
            var aTimeLineData = new TimeLineData
            {
                Chirps = timeLine,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            };
            return aTimeLineData;
        }
    }
}