
namespace JavaGeneration.Chirper.Controllers
{
  using System;
  using System.Security.Principal;
  using System.Web.Mvc;
  using System.Web.Security;

  using Models;

  [HandleError]
  public class AccountController : Controller
  {
    private readonly AccountInputValidator validator;

    public AccountController()
      : this(null, null)
    {
    }

    public AccountController(IAuthentificationService authentificationService, Repository repository)
    {
      Repository = repository ?? new Repository();
      AuthentificationService = authentificationService ?? new AuthentificationService(Repository);
      validator = new AccountInputValidator(ModelState);
    }

    private IRepository Repository { get; set; }

    private IAuthentificationService AuthentificationService { get; set; }

    public ActionResult LogOn()
    {
      return View();
    }

    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult LogOn(string userName, string password, bool rememberMe, string returnUrl)
    {
      if (!validator.ValidateLogOn(userName, password))
      {
        return View();
      }

      if (!AuthentificationService.SignIn(userName, password, rememberMe))
      {
        ModelState.AddModelError("_FORM", "The username or password provided is incorrect.");
        return View();
      }

      if (!string.IsNullOrEmpty(returnUrl))
      {
        return Redirect(returnUrl);
      }

      return RedirectToAction("Index", "Home");
    }

    public ActionResult LogOff()
    {
      AuthentificationService.SignOut();
      return RedirectToAction("Index", "Home");
    }

    public ActionResult Register()
    {
      return View();
    }

    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult Register(string userName, string email, string password, string confirmPassword)
    {
      if (validator.ValidateRegistration(userName, email, password, confirmPassword))
      {
        var status = AuthentificationService.SignUp(userName, password, email);
        if (status == MembershipCreateStatus.Success)
        {
          AuthentificationService.SignIn(userName, password, false);
          return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("_FORM", AuthentificationStatus.ToString(status));
      }

      // If we got this far, something failed, redisplay form
      return View();
    }

    [Authorize]
    public ActionResult ChangePassword()
    {
      return View();
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
      if (!validator.ValidateChangePassword(currentPassword, newPassword, confirmPassword))
      {
        return View();
      }

      try
      {
        if (AuthentificationService.ChangePassword(User.Identity.Name, currentPassword, newPassword))
        {
          return View("ChangePasswordSuccess");
        }

        ModelState.AddModelError("_FORM", "The current password is incorrect or the new password is invalid.");
        return View();
      }
      catch
      {
        ModelState.AddModelError("_FORM", "The current password is incorrect or the new password is invalid.");
        return View();
      }
    }

    [Authorize]
    public ActionResult UpdateProfile()
    {
      return View(Repository.GetUser(User.Identity.Name));
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult UpdateProfile(string email, string displayName, string web, string location, string bio)
    {
      var user = Repository.GetUser(User.Identity.Name);
      user.Email = email;
      user.DisplayName = displayName;
      user.Web = web;
      user.Location = location;
      user.Bio = bio;
      if (Repository.UpdateUser(user))
      {
        return View("UpdateProfileSuccess");
      }

      return View("Error", new ErrorInfo("Unable to updte user's profile"));
    }

    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      if (filterContext.HttpContext.User.Identity is WindowsIdentity)
      {
        throw new InvalidOperationException("Windows authentication is not supported.");
      }
    }
  }
}