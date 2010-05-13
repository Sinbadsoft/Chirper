namespace JavaGeneration.Chirper
{
    using System.Web.Security;

    public interface IAuthentificationService
    {
        int MinPasswordLength { get; }

        bool SignIn(string userName, string password, bool createPersistentCookie);

        void SignOut();

        MembershipCreateStatus CreateUserAndSignIn(string userName, string password, string email);

        bool ChangePassword(string userName, string oldPassword, string newPassword);
    }
}