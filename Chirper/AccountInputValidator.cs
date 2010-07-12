namespace JavaGeneration.Chirper.Controllers
{
  using System;
  using System.Web.Mvc;

  public class AccountInputValidator
  {
    public AccountInputValidator(ModelStateDictionary modelState)
    {
      ModelState = modelState;
    }

    private ModelStateDictionary ModelState { get; set; }

    public bool ValidateChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
      if (string.IsNullOrEmpty(currentPassword))
      {
        ModelState.AddModelError("currentPassword", "You must specify a current password.");
      }

      if (string.IsNullOrEmpty(newPassword))
      {
        ModelState.AddModelError("newPassword", "You must specify a new password .");
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

      if (string.IsNullOrEmpty(password))
      {
        ModelState.AddModelError("password", "You must specify a password.");
      }

      if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
      {
        ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
      }

      return ModelState.IsValid;
    }
  }
}