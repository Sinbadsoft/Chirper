namespace JavaGeneration.Chirper.Controllers
{
  using System.Web.Mvc;
  using Models;

  [HandleError]
  public class HomeController : Controller
  {
    public HomeController()
      : this(new Repository(CassandraClients.Make()))
    {
    }

    private HomeController(IRepository repository)
    {
      Repository = repository;
    }

    private IRepository Repository { get; set; }

    public ActionResult Index()
    {
      if (!User.Identity.IsAuthenticated)
      {
        return PublicTimeLine();
      }

      return TimeLine();
    }

    [Authorize]
    public ActionResult TimeLine()
    {
      return View("Index", Repository.GetTimeLine(User.Identity.Name));
    }

    public ActionResult UserLine(string id)
    {
      if (string.IsNullOrEmpty(id))
      {
        if (User.Identity.IsAuthenticated)
        {
          id = User.Identity.Name;
        }
        else
        {
          return RedirectToAction("LogOn", "Account");
        }
      }

      var user = Repository.GetUser(id);
      if (user == null)
      {
        return View("Error", string.Format("Unknown user {0}.", id));
      }

      ViewData["User"] = user;
      return View("Index", Repository.GetUserLine(id));
    }

    public ActionResult PublicTimeLine()
    {
      ViewData["Public"] = true;
      return View("Index", Repository.GetPublicTimeLine());
    }

    public ActionResult ShowTweet(string id)
    {
      var tweet = Repository.GetTweet(id);
      if (tweet == null)
      {
        return View("Error", "Unable to load tweet.");
      }
      
      ViewData["OneTweet"] = true;
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
  }
}