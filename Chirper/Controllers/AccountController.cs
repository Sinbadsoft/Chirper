namespace JavaGeneration.Chirper.Controllers
{
    using System;
    using System.Security.Principal;
    using System.Web.Mvc;
    using System.Web.Security;
    using System.Collections.Generic;
    using System.Linq;
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

        #region HTTP GET AND POST HANDLERS

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
            User model = Repository.GetUser(User.Identity.Name);
            var genders = new List<Gender>
                              {
                                  new Gender {DisplayName = "Unspecified", Value = "Unspecified"},
                                  new Gender {DisplayName = "Male", Value = "Male"},
                                  new Gender {DisplayName = "Female", Value = "Female"},
                              };
            model.Genders = GenerateGenderDropDownList(genders);
            return View(model);
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UpdateProfile(User updatedUser)
        {
            // TODO: validate model
            var user = Repository.GetUser(User.Identity.Name);
            user.Email = updatedUser.Email; // TODO: we are updating every field every time, this seems unnecessary;
            user.DisplayName = updatedUser.DisplayName;
            user.Web = updatedUser.Web;
            user.Location = updatedUser.Location;
            user.Bio = updatedUser.Bio;
            user.Gender = updatedUser.Gender;
            if (Repository.UpdateUser(user))
            {
                return View("UpdateProfileSuccess");
            }
            return View("Error", new ErrorInfo("Unable to update the user's profile"));
        }

        #endregion

        private static IEnumerable<SelectListItem> GenerateGenderDropDownList(IEnumerable<Gender> items)
        {
            var genderItems =
                items.Select(gender => new SelectListItem {Text = gender.DisplayName, Value = gender.Value}).ToList();
            // small tidbit, add item to start of the list:
            // var allItem = new SelectListItem {Value = "UNKNOWN", Text = "UNKNOWN"};
            // genderItems.Insert(0, allItem);
            return genderItems;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity is WindowsIdentity)
            {
                throw new InvalidOperationException("Windows authentication is not supported.");
            }
        }
    }

    public class Gender
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }
}