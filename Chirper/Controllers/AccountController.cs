namespace JavaGeneration.Chirper.Controllers
{
    using System;
    using System.Security.Principal;
    using System.Web.Mvc;
    using System.Web.Security;

    [HandleError]
    public class AccountController : Controller
    {
        public AccountController()
            : this(null)
        {
        }

        public AccountController(IAuthentificationService authentificationService)
        {
            AuthentificationService = authentificationService ?? new AuthentificationService(CassandraClients.Make());
            Validator = new AccountInputValidator(AuthentificationService, ModelState);
        }

        private AccountInputValidator Validator { get; set; }

        private IAuthentificationService AuthentificationService { get; set; }

        public ActionResult LogOn()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LogOn(string userName, string password, bool rememberMe, string returnUrl)
        {
            if (!Validator.ValidateLogOn(userName, password))
            {
                return View();
            }

            if (!AuthentificationService.SignIn(userName, password, rememberMe))
            {
                ModelState.AddModelError("_FORM", "The username or password provided is incorrect.");
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
            ViewData["PasswordLength"] = AuthentificationService.MinPasswordLength;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Register(string userName, string email, string password, string confirmPassword)
        {
            ViewData["PasswordLength"] = AuthentificationService.MinPasswordLength;
            if (Validator.ValidateRegistration(userName, email, password, confirmPassword))
            {
                var status = AuthentificationService.CreateUserAndSignIn(userName, password, email);
                if (status == MembershipCreateStatus.Success)
                {
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
            ViewData["PasswordLength"] = AuthentificationService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            ViewData["PasswordLength"] = AuthentificationService.MinPasswordLength;
            if (!Validator.ValidateChangePassword(currentPassword, newPassword, confirmPassword))
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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity is WindowsIdentity)
            {
                throw new InvalidOperationException("Windows authentication is not supported.");
            }
        }
     }
}