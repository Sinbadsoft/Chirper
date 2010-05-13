namespace JavaGeneration.Chirper.Controllers
{
    using System;
    using System.Globalization;
    using System.Web.Mvc;

    public class AccountInputValidator
    {
        public AccountInputValidator(IAuthentificationService authentificationService, ModelStateDictionary modelState)
        {
            AuthentificationService = authentificationService;
            ModelState = modelState;
        }

        private IAuthentificationService AuthentificationService { get; set; }
        
        private ModelStateDictionary ModelState { get; set; }

        public bool ValidateChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword))
            {
                ModelState.AddModelError("currentPassword", "You must specify a current password.");
            }
            
            if (newPassword == null || newPassword.Length < AuthentificationService.MinPasswordLength)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    "You must specify a new password of {0} or more characters.",
                    AuthentificationService.MinPasswordLength);
                ModelState.AddModelError("newPassword", message);
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
            }

            return ModelState.IsValid;
        }

        public bool ValidateLogOn(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("username", "You must specify a username.");
            }

            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "You must specify a password.");
            }

           return ModelState.IsValid;
        }

        public bool ValidateRegistration(string userName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("username", "You must specify a username.");
            }
            
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", "You must specify an email address.");
            }

            if (password == null || password.Length < AuthentificationService.MinPasswordLength)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    "You must specify a password of {0} or more characters.",
                    AuthentificationService.MinPasswordLength);
                ModelState.AddModelError("password", message);
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
            }

            return ModelState.IsValid;
        }
    }
}