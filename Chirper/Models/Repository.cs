using System.Globalization;
using System.Text;
using Apache.Cassandra;
using System;
using System.Collections.Generic;

namespace JavaGeneration.Chirper.Models
{
    public class Repository : IRepository
    {
        private const string TimeLineFamilyName = "TimeLine";
        private const string UserLineFamilyName = "UserLine";
        private const string FollowingFamilyName = "Following";
        private const string FollowersFamilyName = "Followers";
        private const string TweetsFamilyName = "Tweets";
        private const string UsersFamilyName = "Users";

        private const int MaxTimelineTweets = 150;
        private const int MaxRelatedUsers = 5000;
        private const string PublicTimeLineUser = "!PUBLIC!";

        public Repository()
        {
            KeySpace = new KeySpace("Test Cluster", "Chirper");
        }

        internal KeySpace KeySpace { get; set; }

        public IList<Tweet> GetPublicTimeLine()
        {
            return GetTimeLine(PublicTimeLineUser);
        }

        public IList<Tweet> GetTimeLine(string userName)
        {
            return GetTimeLine(userName, TimeLineFamilyName);
        }

        public IList<Tweet> GetUserLine(string userName)
        {
            return GetTimeLine(userName, UserLineFamilyName);
        }

        protected IList<Tweet> GetTimeLine(string userName, string type)
        {
            return GetUserItems(
                userName, 
                type, 
                c => GetTweet(Encoding.ASCII.GetString(c.Column.Value)),
                MaxTimelineTweets,
                (x, y) => x.Time.CompareTo(y.Time));
        }

        public IList<Following> GetFollowing(string userName)
        {
            return GetUserItems(
                userName,
                FollowingFamilyName,
                c =>
                    {
                        string name = Encoding.ASCII.GetString(c.Column.Name);
                        return new Following
                        {
                            User = GetUser(name),
                            Since = TimestampToDate(Encoding.ASCII.GetString(c.Column.Value))
                        };
                    },
                MaxRelatedUsers,
                (f1, f2) => f1.Since.CompareTo(f2.Since));
        }

        public IList<Follower> GetFollowers(string userName)
        {
            return GetUserItems(
                userName,
                FollowersFamilyName,
                c => new Follower
                         {
                             User = GetUser(Encoding.ASCII.GetString(c.Column.Name)),
                             Since = TimestampToDate(Encoding.ASCII.GetString(c.Column.Value))
                         },
                MaxRelatedUsers,
                (f1, f2) => f1.Since.CompareTo(f2.Since));
        }

        public bool Follow(string follower, string following)
        {
            if (string.Equals(follower, following))
            {
                return false;
            }

            var followerUser = GetUser(follower);
            var followingUser = GetUser(following);
            if (followerUser == null || followingUser == null)
            {
                return false;
            }

            var followTimestamp = DateToTimestamp(DateTime.UtcNow);
            if (!KeySpace.Exists(FollowersFamilyName, following, follower))
            {
                KeySpace.Insert(FollowersFamilyName, following, follower, followTimestamp);
            }

            if (!KeySpace.Exists(FollowingFamilyName, follower, following))
            {
                KeySpace.Insert(FollowingFamilyName, follower, following, followTimestamp);
            }

            return true;
        }

        public bool UnFollow(string follower, string following)
        {
            throw new NotImplementedException();
        }

        public User GetUser(string userName)
        {
            if (!KeySpace.Exists(UsersFamilyName, userName, UsersFamily.Name))
            {
                return null;
            }

            return new User
                       {
                           Name = userName,
                           Gender = KeySpace.GetValue(UsersFamilyName, userName, UsersFamily.Gender),
                           PasswordHash = KeySpace.GetValue(UsersFamilyName, userName, UsersFamily.PasswordHash),
                           Bio = KeySpace.GetValue(UsersFamilyName, userName, UsersFamily.Bio),
                           DisplayName = KeySpace.GetValue(UsersFamilyName, userName, UsersFamily.DisplayName),
                           Email = KeySpace.GetValue(UsersFamilyName, userName, UsersFamily.Email),
                           Location = KeySpace.GetValue(UsersFamilyName, userName, UsersFamily.Location),
                           Web = KeySpace.GetValue(UsersFamilyName, userName, UsersFamily.Web),
                           CreatedAt =
                               TimestampToDate(KeySpace.GetValue(UsersFamilyName, userName, UsersFamily.CreatedAt))
                       };
        }

        public Tweet GetTweet(string id)
        {
            if (!KeySpace.Exists(TweetsFamilyName, id, TweetsFamily.Id))
            {
                return null;
            }

            return new Tweet
                       {
                           Id = id,
                           User = KeySpace.GetValue(TweetsFamilyName, id, TweetsFamily.User) ?? string.Empty,
                           InReplyToTweet =
                               KeySpace.GetValue(TweetsFamilyName, id, TweetsFamily.InReplyToTweet) ?? string.Empty,
                           InReplyToUser =
                               KeySpace.GetValue(TweetsFamilyName, id, TweetsFamily.InReplyToUser) ?? string.Empty,
                           Location = KeySpace.GetValue(TweetsFamilyName, id, TweetsFamily.Location) ?? string.Empty,
                           Text = KeySpace.GetValue(TweetsFamilyName, id, TweetsFamily.Text) ?? string.Empty,
                           Time =
                               TimestampToDate(KeySpace.GetValue(TweetsFamilyName, id, TweetsFamily.Time) ??
                                               string.Empty)
                       };
        }

        public void AddTweet(Tweet tweet)
        {
            tweet.Id = Guid.NewGuid().ToString();
            tweet.Time = DateTime.UtcNow;
            var tweetTimestamp = DateToTimestamp(tweet.Time);
            KeySpace.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.Id, tweet.Id);
            KeySpace.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.Location, tweet.Location);
            KeySpace.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.Text, tweet.Text);
            KeySpace.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.Time, tweetTimestamp);
            KeySpace.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.User, tweet.User);
            if (!string.IsNullOrEmpty(tweet.InReplyToTweet) && !string.IsNullOrEmpty(tweet.InReplyToUser))
            {
                KeySpace.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.InReplyToTweet, tweet.InReplyToTweet);
                KeySpace.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.InReplyToUser, tweet.InReplyToUser);
            }

            KeySpace.Insert(UserLineFamilyName, tweet.User, tweetTimestamp, tweet.Id);
            KeySpace.Insert(TimeLineFamilyName, tweet.User, tweetTimestamp, tweet.Id);
            KeySpace.Insert(TimeLineFamilyName, PublicTimeLineUser, tweetTimestamp, tweet.Id);

            foreach (var follower in GetFollowers(tweet.User))
            {
                KeySpace.Insert(TimeLineFamilyName, follower.User.Name, tweetTimestamp, tweet.Id);
            }
        }

        public bool AddUser(User user)
        {
            if (KeySpace.Exists(UsersFamilyName, user.Name, UsersFamily.Name))
            {
                return false;
            }

            KeySpace.Insert(UsersFamilyName, user.Name, UsersFamily.Name, user.Name);
            user.CreatedAt = DateTime.UtcNow;
            InsertOrUpdateUser(user);
            return true;
        }

        public bool UpdateUser(User user)
        {
            if (!KeySpace.Exists(UsersFamilyName, user.Name, UsersFamily.Name))
            {
                return false;
            }

            InsertOrUpdateUser(user);
            return true;
        }

        private static DateTime TimestampToDate(string timestamp)
        {
            return string.IsNullOrEmpty(timestamp)
                       ? DateTime.MinValue
                       : new DateTime(Convert.ToInt64(timestamp), DateTimeKind.Utc);
        }

        private static string DateToTimestamp(DateTime date)
        {
            return date.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture);
        }

        private void InsertOrUpdateUser(User user)
        {
            InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Bio, user.Bio);
            InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.CreatedAt, DateToTimestamp(user.CreatedAt));
            InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.DisplayName, user.DisplayName);
            InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Email, user.Email);
            InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Location, user.Location);
            InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Web, user.Web);
            InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Gender, user.Gender);
            InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.PasswordHash, user.PasswordHash);
        }

        private void InsertIfNotNullOrEmpty(string familyName, string key, string columnName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                KeySpace.Insert(familyName, key, columnName, value);
            }
        }

        private List<T> GetUserItems<T>(
            string userName,
            string joinFamily,
            Converter<ColumnOrSuperColumn, T> getValueFromColumn,
            int maxItems,
            Comparison<T> comparison)
        {
            var items = KeySpace.GetSlice(joinFamily, userName, maxItems).ConvertAll(getValueFromColumn);
            items.Sort(comparison);
            if (joinFamily.Equals(UserLineFamilyName) || (joinFamily.Equals(TimeLineFamilyName)))
                items.Reverse();
            return items;
        }
    }
}