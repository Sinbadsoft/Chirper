namespace JavaGeneration.Chirper
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.Odbc;
    using System.Diagnostics;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Security;

    public class CassandraMembershipProvider : MembershipProvider
    {
        private const string DEFAULT_PROVIDER_NAME = "CassandraMembershipProvider";

        private const string DEFAULT_DESCRIPTION = "Sample Cassandra Membership provider";

        /// <summary>
        /// Global connection string, generated password length, generic exception message, event log info.
        /// </summary>
        private int newPasswordLength = 8;

        private string eventSource = DEFAULT_PROVIDER_NAME;

        private string eventLog = "Application";

        private string exceptionMessage = "An exception occurred. Please check the Event Log.";

        private string connectionString;

        /// <summary>
        /// Used when determining encryption key values.
        /// </summary>
        private MachineKeySection machineKey;

        /// <summary>
        /// If false, exceptions are thrown to the caller. If true,
        /// exceptions are written to the event log.
        /// </summary>
        private bool writeExceptionsToEventLog;

        public bool WriteExceptionsToEventLog
        {
            get
            {
                return writeExceptionsToEventLog;
            }
            set
            {
                writeExceptionsToEventLog = value;
            }
        }

        public override string ApplicationName { get; set; }

        public override bool EnablePasswordReset { get; private set; }

        public override bool EnablePasswordRetrieval { get; private set; }

        public override bool RequiresQuestionAndAnswer { get; private set; }

        public override bool RequiresUniqueEmail { get; private set; }

        public override int MaxInvalidPasswordAttempts { get; private set; }

        public override int PasswordAttemptWindow { get; private set; }

        public override MembershipPasswordFormat PasswordFormat { get; private set; }

        public override int MinRequiredNonAlphanumericCharacters { get; private set; }

        public override int MinRequiredPasswordLength { get; private set; }

        public override string PasswordStrengthRegularExpression { get; private set; }

        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            if (!ValidateUser(username, oldPwd))
            {
                return false;
            }

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPwd, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                
                throw new MembershipPasswordException("Change password canceled due to new password validation failure.");
            }

            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "UPDATE Users " + " SET Password = ?, LastPasswordChangedDate = ? " +
                    " WHERE Username = ? AND ApplicationName = ?",
                    conn);

            cmd.Parameters.Add("@Password", OdbcType.VarChar, 255).Value = EncodePassword(newPwd);
            cmd.Parameters.Add("@LastPasswordChangedDate", OdbcType.DateTime).Value = DateTime.Now;
            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            int rowsAffected = 0;

            try
            {
                conn.Open();

                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ChangePassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPwdQuestion, string newPwdAnswer)
        {
            if (!ValidateUser(username, password))
            {
                return false;
            }

            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "UPDATE Users " + " SET PasswordQuestion = ?, PasswordAnswer = ?" +
                    " WHERE Username = ? AND ApplicationName = ?",
                    conn);

            cmd.Parameters.Add("@Question", OdbcType.VarChar, 255).Value = newPwdQuestion;
            cmd.Parameters.Add("@Answer", OdbcType.VarChar, 255).Value = EncodePassword(newPwdAnswer);
            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            int rowsAffected = 0;

            try
            {
                conn.Open();

                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ChangePasswordQuestionAndAnswer");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Initialize values from web.config.
        /// </summary>
        /// <param name="name"> The membership provider name. </param>
        /// <param name="config"> The config collection. </param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = DEFAULT_PROVIDER_NAME;
            }

            if (string.IsNullOrEmpty(config["description"]))
            {
                config["description"] = DEFAULT_DESCRIPTION;
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            this.ApplicationName = GetConfigValue(
                config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            this.MaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            this.PasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            this.MinRequiredNonAlphanumericCharacters =
                Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            this.MinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            this.PasswordStrengthRegularExpression =
                Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], string.Empty));
            this.EnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            this.EnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            this.RequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            this.RequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            writeExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

            string temp_format = config["passwordFormat"] ?? "Hashed";
            switch (temp_format)
            {
                case "Hashed":
                    this.PasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    this.PasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    this.PasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            ConnectionStringSettings ConnectionStringSettings =
                ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = ConnectionStringSettings.ConnectionString;

            // Get encryption and decryption key information from the configuration.
            Configuration cfg =
                WebConfigurationManager.OpenWebConfiguration(
                    System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (machineKey.ValidationKey.Contains("AutoGenerate"))
            {
                if (this.PasswordFormat != MembershipPasswordFormat.Clear)
                {
                    throw new ProviderException(
                        "Hashed or Encrypted passwords " + "are not supported with auto-generated keys.");
                }
            }
        }

        public override MembershipUser CreateUser(
            string username,
            string password,
            string email,
            string passwordQuestion,
            string passwordAnswer,
            bool isApproved,
            object providerUserKey,
            out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && !string.IsNullOrEmpty(GetUserNameByEmail(email)))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }

                OdbcConnection conn = new OdbcConnection(connectionString);
                OdbcCommand cmd =
                    new OdbcCommand(
                        "INSERT INTO Users " + " (PKID, Username, Password, Email, PasswordQuestion, " +
                        " PasswordAnswer, IsApproved," +
                        " Comment, CreationDate, LastPasswordChangedDate, LastActivityDate," +
                        " ApplicationName, IsLockedOut, LastLockedOutDate," +
                        " FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart, " +
                        " FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart)" +
                        " Values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                        conn);

                cmd.Parameters.Add("@PKID", OdbcType.UniqueIdentifier).Value = providerUserKey;
                cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
                cmd.Parameters.Add("@Password", OdbcType.VarChar, 255).Value = EncodePassword(password);
                cmd.Parameters.Add("@Email", OdbcType.VarChar, 128).Value = email;
                cmd.Parameters.Add("@PasswordQuestion", OdbcType.VarChar, 255).Value = passwordQuestion;
                cmd.Parameters.Add("@PasswordAnswer", OdbcType.VarChar, 255).Value = EncodePassword(passwordAnswer);
                cmd.Parameters.Add("@IsApproved", OdbcType.Bit).Value = isApproved;
                cmd.Parameters.Add("@Comment", OdbcType.VarChar, 255).Value = string.Empty;
                cmd.Parameters.Add("@CreationDate", OdbcType.DateTime).Value = createDate;
                cmd.Parameters.Add("@LastPasswordChangedDate", OdbcType.DateTime).Value = createDate;
                cmd.Parameters.Add("@LastActivityDate", OdbcType.DateTime).Value = createDate;
                cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;
                cmd.Parameters.Add("@IsLockedOut", OdbcType.Bit).Value = false;
                cmd.Parameters.Add("@LastLockedOutDate", OdbcType.DateTime).Value = createDate;
                cmd.Parameters.Add("@FailedPasswordAttemptCount", OdbcType.Int).Value = 0;
                cmd.Parameters.Add("@FailedPasswordAttemptWindowStart", OdbcType.DateTime).Value = createDate;
                cmd.Parameters.Add("@FailedPasswordAnswerAttemptCount", OdbcType.Int).Value = 0;
                cmd.Parameters.Add("@FailedPasswordAnswerAttemptWindowStart", OdbcType.DateTime).Value = createDate;

                try
                {
                    conn.Open();

                    int recAdded = cmd.ExecuteNonQuery();

                    status = recAdded > 0 ? MembershipCreateStatus.Success : MembershipCreateStatus.UserRejected;
                }
                catch (OdbcException e)
                {
                    if (WriteExceptionsToEventLog)
                    {
                        WriteToEventLog(e, "CreateUser");
                    }

                    status = MembershipCreateStatus.ProviderError;
                }
                finally
                {
                    conn.Close();
                }

                return GetUser(username, false);
            }
            status = MembershipCreateStatus.DuplicateUserName;

            return null;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd = new OdbcCommand(
                "DELETE FROM Users " + " WHERE Username = ? AND Applicationname = ?", conn);

            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            int rowsAffected = 0;

            try
            {
                conn.Open();

                rowsAffected = cmd.ExecuteNonQuery();

                if (deleteAllRelatedData)
                {
                    // Process commands to delete all data for the user in the database.
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd = new OdbcCommand("SELECT Count(*) FROM Users " + "WHERE ApplicationName = ?", conn);
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            MembershipUserCollection users = new MembershipUserCollection();

            OdbcDataReader reader = null;
            totalRecords = 0;

            try
            {
                conn.Open();
                totalRecords = (int)cmd.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }

                cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
                                  " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                                  " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " + " FROM Users " +
                                  " WHERE ApplicationName = ? " + " ORDER BY Username Asc";

                reader = cmd.ExecuteReader();

                int counter = 0;
                int startIndex = pageSize * pageIndex;
                int endIndex = startIndex + pageSize - 1;

                while (reader.Read())
                {
                    if (counter >= startIndex)
                    {
                        MembershipUser u = GetUserFromReader(reader);
                        users.Add(u);
                    }

                    if (counter >= endIndex)
                    {
                        cmd.Cancel();
                    }

                    counter++;
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetAllUsers ");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }

            return users;
        }

        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "SELECT Count(*) FROM Users " + " WHERE LastActivityDate > ? AND ApplicationName = ?", conn);

            cmd.Parameters.Add("@CompareDate", OdbcType.DateTime).Value = compareTime;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            int numOnline = 0;

            try
            {
                conn.Open();

                numOnline = (int)cmd.ExecuteScalar();
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetNumberOfUsersOnline");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            return numOnline;
        }

        public override string GetPassword(string username, string answer)
        {
            if (!this.EnablePasswordRetrieval)
            {
                throw new ProviderException("Password Retrieval Not Enabled.");
            }

            if (this.PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Cannot retrieve Hashed passwords.");
            }

            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "SELECT Password, PasswordAnswer, IsLockedOut FROM Users " +
                    " WHERE Username = ? AND ApplicationName = ?",
                    conn);

            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            string password = string.Empty;
            string passwordAnswer = string.Empty;
            OdbcDataReader reader = null;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.HasRows)
                {
                    reader.Read();

                    if (reader.GetBoolean(2))
                    {
                        throw new MembershipPasswordException("The supplied user is locked out.");
                    }

                    password = reader.GetString(0);
                    passwordAnswer = reader.GetString(1);
                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetPassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }

            if (this.RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new MembershipPasswordException("Incorrect password answer.");
            }

            if (this.PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = UnEncodePassword(password);
            }

            return password;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "SELECT PKID, Username, Email, PasswordQuestion," +
                    " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                    " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" +
                    " FROM Users WHERE Username = ? AND ApplicationName = ?",
                    conn);

            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            MembershipUser u = null;
            OdbcDataReader reader = null;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    u = GetUserFromReader(reader);

                    if (userIsOnline)
                    {
                        OdbcCommand updateCmd =
                            new OdbcCommand(
                                "UPDATE Users " + "SET LastActivityDate = ? " +
                                "WHERE Username = ? AND Applicationname = ?",
                                conn);

                        updateCmd.Parameters.Add("@LastActivityDate", OdbcType.DateTime).Value = DateTime.Now;
                        updateCmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
                        updateCmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUser(String, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                conn.Close();
            }

            return u;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "SELECT PKID, Username, Email, PasswordQuestion," +
                    " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                    " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate" + " FROM Users WHERE PKID = ?",
                    conn);

            cmd.Parameters.Add("@PKID", OdbcType.UniqueIdentifier).Value = providerUserKey;

            MembershipUser u = null;
            OdbcDataReader reader = null;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    u = GetUserFromReader(reader);

                    if (userIsOnline)
                    {
                        OdbcCommand updateCmd =
                            new OdbcCommand("UPDATE Users " + "SET LastActivityDate = ? " + "WHERE PKID = ?", conn);

                        updateCmd.Parameters.Add("@LastActivityDate", OdbcType.DateTime).Value = DateTime.Now;
                        updateCmd.Parameters.Add("@PKID", OdbcType.UniqueIdentifier).Value = providerUserKey;

                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUser(Object, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                conn.Close();
            }

            return u;
        }

        public override bool UnlockUser(string username)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "UPDATE Users " + " SET IsLockedOut = False, LastLockedOutDate = ? " +
                    " WHERE Username = ? AND ApplicationName = ?",
                    conn);

            cmd.Parameters.Add("@LastLockedOutDate", OdbcType.DateTime).Value = DateTime.Now;
            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            int rowsAffected = 0;

            try
            {
                conn.Open();

                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UnlockUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public override string GetUserNameByEmail(string email)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd = new OdbcCommand(
                "SELECT Username" + " FROM Users WHERE Email = ? AND ApplicationName = ?", conn);

            cmd.Parameters.Add("@Email", OdbcType.VarChar, 128).Value = email;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            string username = string.Empty;

            try
            {
                conn.Open();

                username = (string)cmd.ExecuteScalar();
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUserNameByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }

            if (username == null)
            {
                username = "";
            }

            return username;
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!this.EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not enabled.");
            }

            if (answer == null && this.RequiresQuestionAndAnswer)
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword = Membership.GeneratePassword(
                newPasswordLength, this.MinRequiredNonAlphanumericCharacters);

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                else
                {
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");
                }
            }

            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "SELECT PasswordAnswer, IsLockedOut FROM Users " + " WHERE Username = ? AND ApplicationName = ?",
                    conn);

            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            int rowsAffected = 0;
            string passwordAnswer = "";
            OdbcDataReader reader = null;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.HasRows)
                {
                    reader.Read();

                    if (reader.GetBoolean(1))
                    {
                        throw new MembershipPasswordException("The supplied user is locked out.");
                    }

                    passwordAnswer = reader.GetString(0);
                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
                }

                if (this.RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                {
                    UpdateFailureCount(username, "passwordAnswer");

                    throw new MembershipPasswordException("Incorrect password answer.");
                }

                OdbcCommand updateCmd =
                    new OdbcCommand(
                        "UPDATE Users " + " SET Password = ?, LastPasswordChangedDate = ?" +
                        " WHERE Username = ? AND ApplicationName = ? AND IsLockedOut = False",
                        conn);

                updateCmd.Parameters.Add("@Password", OdbcType.VarChar, 255).Value = EncodePassword(newPassword);
                updateCmd.Parameters.Add("@LastPasswordChangedDate", OdbcType.DateTime).Value = DateTime.Now;
                updateCmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
                updateCmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

                rowsAffected = updateCmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ResetPassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }

            if (rowsAffected > 0)
            {
                return newPassword;
            }
            else
            {
                throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
            }
        }

        public override void UpdateUser(MembershipUser user)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "UPDATE Users " + " SET Email = ?, Comment = ?," + " IsApproved = ?" +
                    " WHERE Username = ? AND ApplicationName = ?",
                    conn);

            cmd.Parameters.Add("@Email", OdbcType.VarChar, 128).Value = user.Email;
            cmd.Parameters.Add("@Comment", OdbcType.VarChar, 255).Value = user.Comment;
            cmd.Parameters.Add("@IsApproved", OdbcType.Bit).Value = user.IsApproved;
            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = user.UserName;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UpdateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "SELECT Password, IsApproved FROM Users " +
                    " WHERE Username = ? AND ApplicationName = ? AND IsLockedOut = False",
                    conn);

            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            OdbcDataReader reader = null;
            bool isApproved = false;
            string pwd = string.Empty;

            try
            {
                conn.Open();
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.HasRows)
                {
                    reader.Read();
                    pwd = reader.GetString(0);
                    isApproved = reader.GetBoolean(1);
                }
                else
                {
                    return false;
                }

                reader.Close();

                if (CheckPassword(password, pwd))
                {
                    if (isApproved)
                    {
                        isValid = true;

                        OdbcCommand updateCmd =
                            new OdbcCommand(
                                "UPDATE Users SET LastLoginDate = ?" + " WHERE Username = ? AND ApplicationName = ?",
                                conn);

                        updateCmd.Parameters.Add("@LastLoginDate", OdbcType.DateTime).Value = DateTime.Now;
                        updateCmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
                        updateCmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;
                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {  
                    conn.Close();

                    UpdateFailureCount(username, "password");
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ValidateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                conn.Close();
            }

            return isValid;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand("SELECT Count(*) FROM Users " + "WHERE Username LIKE ? AND ApplicationName = ?", conn);
            cmd.Parameters.Add("@UsernameSearch", OdbcType.VarChar, 255).Value = usernameToMatch;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            MembershipUserCollection users = new MembershipUserCollection();

            OdbcDataReader reader = null;

            try
            {
                conn.Open();
                totalRecords = (int)cmd.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }

                cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
                                  " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                                  " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " + " FROM Users " +
                                  " WHERE Username LIKE ? AND ApplicationName = ? " + " ORDER BY Username Asc";

                reader = cmd.ExecuteReader();

                int counter = 0;
                int startIndex = pageSize * pageIndex;
                int endIndex = startIndex + pageSize - 1;

                while (reader.Read())
                {
                    if (counter >= startIndex)
                    {
                        MembershipUser u = GetUserFromReader(reader);
                        users.Add(u);
                    }

                    if (counter >= endIndex)
                    {
                        cmd.Cancel();
                    }

                    counter++;
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersByName");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                conn.Close();
            }

            return users;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand("SELECT Count(*) FROM Users " + "WHERE Email LIKE ? AND ApplicationName = ?", conn);
            cmd.Parameters.Add("@EmailSearch", OdbcType.VarChar, 255).Value = emailToMatch;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            MembershipUserCollection users = new MembershipUserCollection();

            OdbcDataReader reader = null;
            totalRecords = 0;

            try
            {
                conn.Open();
                totalRecords = (int)cmd.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }

                cmd.CommandText = "SELECT PKID, Username, Email, PasswordQuestion," +
                                  " Comment, IsApproved, IsLockedOut, CreationDate, LastLoginDate," +
                                  " LastActivityDate, LastPasswordChangedDate, LastLockedOutDate " + " FROM Users " +
                                  " WHERE Email LIKE ? AND ApplicationName = ? " + " ORDER BY Username Asc";

                reader = cmd.ExecuteReader();

                int counter = 0;
                int startIndex = pageSize * pageIndex;
                int endIndex = startIndex + pageSize - 1;

                while (reader.Read())
                {
                    if (counter >= startIndex)
                    {
                        MembershipUser u = GetUserFromReader(reader);
                        users.Add(u);
                    }

                    if (counter >= endIndex)
                    {
                        cmd.Cancel();
                    }

                    counter++;
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                conn.Close();
            }

            return users;
        }

        /// <summary>
        /// GetUserFromReader
        ///    A helper function that takes the current row from the OdbcDataReader
        /// and hydrates a MembershiUser from the values. Called by the 
        /// MembershipUser.GetUser implementation.
        /// </summary>
        private MembershipUser GetUserFromReader(OdbcDataReader reader)
        {
            object providerUserKey = reader.GetValue(0);
            string username = reader.GetString(1);
            string email = reader.GetString(2);

            string passwordQuestion = string.Empty;
            if (reader.GetValue(3) != DBNull.Value)
            {
                passwordQuestion = reader.GetString(3);
            }

            string comment = string.Empty;
            if (reader.GetValue(4) != DBNull.Value)
            {
                comment = reader.GetString(4);
            }

            bool isApproved = reader.GetBoolean(5);
            bool isLockedOut = reader.GetBoolean(6);
            DateTime creationDate = reader.GetDateTime(7);

            DateTime lastLoginDate = new DateTime();
            if (reader.GetValue(8) != DBNull.Value)
            {
                lastLoginDate = reader.GetDateTime(8);
            }

            DateTime lastActivityDate = reader.GetDateTime(9);
            DateTime lastPasswordChangedDate = reader.GetDateTime(10);

            DateTime lastLockedOutDate = new DateTime();
            if (reader.GetValue(11) != DBNull.Value)
            {
                lastLockedOutDate = reader.GetDateTime(11);
            }

            MembershipUser u = new MembershipUser(
                this.Name,
                username,
                providerUserKey,
                email,
                passwordQuestion,
                comment,
                isApproved,
                isLockedOut,
                creationDate,
                lastLoginDate,
                lastActivityDate,
                lastPasswordChangedDate,
                lastLockedOutDate);

            return u;
        }

        //
        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        //
        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

        /// <summary>
        /// UpdateFailureCount
        ///   A helper method that performs the checks and updates associated with
        /// password failure tracking.
        /// </summary>
        private void UpdateFailureCount(string username, string failureType)
        {
            OdbcConnection conn = new OdbcConnection(connectionString);
            OdbcCommand cmd =
                new OdbcCommand(
                    "SELECT FailedPasswordAttemptCount, " + "  FailedPasswordAttemptWindowStart, " +
                    "  FailedPasswordAnswerAttemptCount, " + "  FailedPasswordAnswerAttemptWindowStart " +
                    "  FROM Users " + "  WHERE Username = ? AND ApplicationName = ?",
                    conn);

            cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

            OdbcDataReader reader = null;
            DateTime windowStart = new DateTime();
            int failureCount = 0;

            try
            {
                conn.Open();

                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.HasRows)
                {
                    reader.Read();

                    if (failureType == "password")
                    {
                        failureCount = reader.GetInt32(0);
                        windowStart = reader.GetDateTime(1);
                    }

                    if (failureType == "passwordAnswer")
                    {
                        failureCount = reader.GetInt32(2);
                        windowStart = reader.GetDateTime(3);
                    }
                }

                reader.Close();

                DateTime windowEnd = windowStart.AddMinutes(this.PasswordAttemptWindow);

                if (failureCount == 0 || DateTime.Now > windowEnd)
                {
                    // First password failure or outside of PasswordAttemptWindow. 
                    // Start a new password failure count from 1 and a new window starting now.

                    if (failureType == "password")
                    {
                        cmd.CommandText = "UPDATE Users " + "  SET FailedPasswordAttemptCount = ?, " +
                                          "      FailedPasswordAttemptWindowStart = ? " +
                                          "  WHERE Username = ? AND ApplicationName = ?";
                    }

                    if (failureType == "passwordAnswer")
                    {
                        cmd.CommandText = "UPDATE Users " + "  SET FailedPasswordAnswerAttemptCount = ?, " +
                                          "      FailedPasswordAnswerAttemptWindowStart = ? " +
                                          "  WHERE Username = ? AND ApplicationName = ?";
                    }

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("@Count", OdbcType.Int).Value = 1;
                    cmd.Parameters.Add("@WindowStart", OdbcType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
                    cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        throw new ProviderException("Unable to update failure count and window start.");
                    }
                }
                else
                {
                    if (failureCount++ >= this.MaxInvalidPasswordAttempts)
                    {
                        // Password attempts have exceeded the failure threshold. Lock out
                        // the user.

                        cmd.CommandText = "UPDATE Users " + "  SET IsLockedOut = ?, LastLockedOutDate = ? " +
                                          "  WHERE Username = ? AND ApplicationName = ?";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add("@IsLockedOut", OdbcType.Bit).Value = true;
                        cmd.Parameters.Add("@LastLockedOutDate", OdbcType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
                        cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

                        if (cmd.ExecuteNonQuery() < 0)
                        {
                            throw new ProviderException("Unable to lock out user.");
                        }
                    }
                    else
                    {
                        // Password attempts have not exceeded the failure threshold. Update
                        // the failure counts. Leave the window the same.

                        if (failureType == "password")
                        {
                            cmd.CommandText = "UPDATE Users " + "  SET FailedPasswordAttemptCount = ?" +
                                              "  WHERE Username = ? AND ApplicationName = ?";
                        }

                        if (failureType == "passwordAnswer")
                        {
                            cmd.CommandText = "UPDATE Users " + "  SET FailedPasswordAnswerAttemptCount = ?" +
                                              "  WHERE Username = ? AND ApplicationName = ?";
                        }

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add("@Count", OdbcType.Int).Value = failureCount;
                        cmd.Parameters.Add("@Username", OdbcType.VarChar, 255).Value = username;
                        cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = this.ApplicationName;

                        if (cmd.ExecuteNonQuery() < 0)
                        {
                            throw new ProviderException("Unable to update failure count.");
                        }
                    }
                }
            }
            catch (OdbcException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UpdateFailureCount");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }
        }

        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (this.PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (this.PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword = Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(machineKey.ValidationKey);
                    encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (this.PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password = Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        //
        // HexToByte
        //   Converts a hexadecimal string to a byte array. Used to convert encryption
        // key values from the configuration.
        //
        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }
        
        private static T GetConfigValue<T>(string configValue, T defaultValue)
        {
            if (string.IsNullOrEmpty(configValue))
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(configValue, typeof(T));
        }
    }
}