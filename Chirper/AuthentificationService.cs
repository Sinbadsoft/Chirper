namespace JavaGeneration.Chirper
{
    using System.Web.Security;
    using HectorSharp;

    public class AuthentificationService : IAuthentificationService
    {
        private static readonly ColumnPath UsersPasswordPath = new ColumnPath("Users", null, "Password");
        private static readonly ColumnPath UsersNamePath = new ColumnPath("Users", null, "Name");
        private static readonly ColumnPath UsersEmailPath = new ColumnPath("Users", null, "Email");
        private readonly ICassandraClient client;

        public AuthentificationService(ICassandraClient cassandraClient)
        {
            client = cassandraClient;
        }

        public int MinPasswordLength
        {
            get { return 10; }
        }

        private IKeyspace Keyspace
        {
            get { return client.GetKeyspace("Chirper"); }
        }

        public bool SignIn(string userName, string password, bool createPersistentCookie)
        {
            if (!ValidateUser(userName, password))
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

        public MembershipCreateStatus CreateUserAndSignIn(string userName, string password, string email)
        {
            Column nameColumn;
            if (Keyspace.TryGetColumn(userName, UsersNamePath, out nameColumn))
            {
                return MembershipCreateStatus.DuplicateUserName;
            }

            Keyspace.Insert(userName, UsersNamePath, userName);
            Keyspace.Insert(userName, UsersPasswordPath, password);
            Keyspace.Insert(userName, UsersEmailPath, email);
            return MembershipCreateStatus.Success;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (!ValidateUser(userName, oldPassword))
            {
                return false;
            }

            Keyspace.Insert(userName, UsersPasswordPath, newPassword);
            return true;
        }

        private bool ValidateUser(string userName, string password)
        {
            Column passwordColumn;
            if (Keyspace.TryGetColumn(userName, UsersPasswordPath, out passwordColumn))
            {
                return string.Equals(password, passwordColumn.Value);
            }

            return false;
        }
    }
}