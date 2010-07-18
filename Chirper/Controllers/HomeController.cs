namespace JavaGeneration.Chirper.Controllers
{
  using System.Web.Mvc;
  using Models;

  [HandleError]
  public class HomeController : Controller
  {
    public HomeController() : this(new Repository())
    {
    }

    private HomeController(IRepository repository)
    {
      Repository = repository;
    }

    private IRepository Repository { get; set; }

    public ActionResult Index()
    {
      return !User.Identity.IsAuthenticated ? PublicTimeLine() : TimeLine();
    }

    [Authorize]
    public ActionResult TimeLine()
    {
      ViewData["Title"] = string.Format("{0} Timeline", User.Identity.Name);
      return View("Index", Repository.GetTimeLine(User.Identity.Name));
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
      return View("Index", Repository.GetUserLine(user.Name));
    }

    public ActionResult PublicTimeLine()
    {
      ViewData["Title"] = "Public Timeline";
      return View("Index", Repository.GetPublicTimeLine());
    }

    public ActionResult ShowTweet(string id)
    {
      var tweet = Repository.GetTweet(id);
      if (tweet == null)
      {
        return View("Error", new ErrorInfo("Unable to load tweet.", null));
      }
      
      ViewData["Title"] = "Show Tweet";
      ViewData["User"] = Repository.GetUser(tweet.User);
      return View("Index", new[] { tweet });
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
      if(!Repository.Follow(User.Identity.Name, id))
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
  }
}