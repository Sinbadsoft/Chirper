namespace JavaGeneration.Chirper
{
  using System.Web.Security;
  using Models;

  public class AuthentificationService : IAuthentificationService
  {
    public AuthentificationService(IRepository cassandraClient)
    {
      Repository = cassandraClient;
    }

    private IRepository Repository { get; set; }

    public bool SignIn(string userName, string password, bool createPersistentCookie)
    {
      var user = Repository.GetUser(userName);
      if (user == null || !string.Equals(user.PasswordHash, password))
      {
        return false;
      }

      FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
      return true;
    }

    public void SignOut()
    {
      FormsAuthentication.SignOut();
    }

    public MembershipCreateStatus SignUp(string userName, string password, string email)
    {
      var user = new User { Name = userName, PasswordHash = password, Email = email };
      return !Repository.AddUser(user)
        ? MembershipCreateStatus.DuplicateUserName
        : MembershipCreateStatus.Success;
    }

    public bool ChangePassword(string userName, string oldPassword, string newPassword)
    {
      var user = Repository.GetUser(userName);
      if (user == null || !string.Equals(user.PasswordHash, oldPassword))
      {
        return false;
      }

      user.PasswordHash = newPassword;
      Repository.UpdateUser(user);
      return true;
    }
  }
}