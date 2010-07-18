namespace JavaGeneration.Chirper.Models
{
  using System;
  using System.Collections.Generic;
  using Aquiles.Model;


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
      Keysapce = new KeySpace("Cluster", "Chirper");
    }

    internal KeySpace Keysapce { get; set; }

    public IList<Tweet> GetPublicTimeLine()
    {
      return GetTimeLine(PublicTimeLineUser);
    }

    public IList<Tweet> GetTimeLine(string userName)
    {
      return GetUserItems(
        userName, TimeLineFamilyName, c => GetTweet(c.Value), MaxTimelineTweets, (x, y) => x.Time.CompareTo(y.Time));
    }

    public IList<Tweet> GetUserLine(string userName)
    {
      return GetUserItems(
        userName, UserLineFamilyName, c => GetTweet(c.Value), MaxTimelineTweets, (x, y) => x.Time.CompareTo(y.Time));
    }

    public IList<Following> GetFollowing(string userName)
    {
      return GetUserItems(
        userName,
        FollowingFamilyName,
        c => new Following { User = GetUser(c.ColumnName), Since = TimestampToDate(c.Value) },
        MaxRelatedUsers,
        (f1, f2) => f1.Since.CompareTo(f2.Since));
    }

    public IList<Follower> GetFollowers(string userName)
    {
      return GetUserItems(
        userName,
        FollowersFamilyName,
        c => new Follower { User = GetUser(c.ColumnName), Since = TimestampToDate(c.Value) },
        MaxRelatedUsers,
        (f1, f2) => f1.Since.CompareTo(f2.Since));
    }

    public bool Follow(string follower, string following)
    {
      if(string.Equals(follower, following))
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
      if(!Keysapce.Exists(FollowersFamilyName, following, follower))
      {
        Keysapce.Insert(FollowersFamilyName, following, follower, followTimestamp);  
      }

      if (!Keysapce.Exists(FollowingFamilyName, follower, following))
      {
        Keysapce.Insert(FollowingFamilyName, follower, following, followTimestamp);
      }

      return true;
    }

    public bool UnFollow(string follower, string following)
    {
      throw new NotImplementedException();
    }

    public User GetUser(string userName)
    {
      if (!Keysapce.Exists(UsersFamilyName, userName, UsersFamily.Name))
      {
        return null;
      }

      return new User
        {
          Name = userName,
          PasswordHash = Keysapce.GetValue(UsersFamilyName, userName, UsersFamily.PasswordHash),
          Bio = Keysapce.GetValue(UsersFamilyName, userName, UsersFamily.Bio),
          DisplayName = Keysapce.GetValue(UsersFamilyName, userName, UsersFamily.DisplayName),
          Email = Keysapce.GetValue(UsersFamilyName, userName, UsersFamily.Email),
          Location = Keysapce.GetValue(UsersFamilyName, userName, UsersFamily.Location),
          Web = Keysapce.GetValue(UsersFamilyName, userName, UsersFamily.Web),
          CreatedAt = TimestampToDate(Keysapce.GetValue(UsersFamilyName, userName, UsersFamily.CreatedAt))
        };
    }

    public Tweet GetTweet(string id)
    {
      if (!Keysapce.Exists(TweetsFamilyName, id, TweetsFamily.Id))
      {
        return null;
      }

      return new Tweet
        {
          Id = id, 
          User = Keysapce.GetValue(TweetsFamilyName, id, TweetsFamily.User) ?? string.Empty,
          InReplyToTweet = Keysapce.GetValue(TweetsFamilyName, id, TweetsFamily.InReplyToTweet) ?? string.Empty,
          InReplyToUser = Keysapce.GetValue(TweetsFamilyName, id, TweetsFamily.InReplyToUser) ?? string.Empty,
          Location = Keysapce.GetValue(TweetsFamilyName, id, TweetsFamily.Location) ?? string.Empty,
          Text = Keysapce.GetValue(TweetsFamilyName, id, TweetsFamily.Text) ?? string.Empty,
          Time = TimestampToDate(Keysapce.GetValue(TweetsFamilyName, id, TweetsFamily.Time) ?? string.Empty)
        };
    }

    public void AddTweet(Tweet tweet)
    {
      tweet.Id = Guid.NewGuid().ToString();
      tweet.Time = DateTime.UtcNow;
      var tweetTimestamp = DateToTimestamp(tweet.Time);
      Keysapce.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.Id, tweet.Id);
      Keysapce.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.Location, tweet.Location);
      Keysapce.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.Text, tweet.Text);
      Keysapce.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.Time, tweetTimestamp);
      Keysapce.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.User, tweet.User);
      if (!string.IsNullOrEmpty(tweet.InReplyToTweet) && !string.IsNullOrEmpty(tweet.InReplyToUser))
      {
        Keysapce.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.InReplyToTweet, tweet.InReplyToTweet);
        Keysapce.Insert(TweetsFamilyName, tweet.Id, TweetsFamily.InReplyToUser, tweet.InReplyToUser);
      }

      Keysapce.Insert(UserLineFamilyName, tweet.User, tweetTimestamp, tweet.Id);
      Keysapce.Insert(TimeLineFamilyName, tweet.User, tweetTimestamp, tweet.Id);
      Keysapce.Insert(TimeLineFamilyName, PublicTimeLineUser, tweetTimestamp, tweet.Id);

      foreach (var follower in GetFollowers(tweet.User))
      {
        Keysapce.Insert(TimeLineFamilyName, follower.User.Name, tweetTimestamp, tweet.Id);
      }
    }

    public bool AddUser(User user)
    {
      if (Keysapce.Exists(UsersFamilyName, user.Name, UsersFamily.Name))
      {
        return false;
      }

      Keysapce.Insert(UsersFamilyName, user.Name, UsersFamily.Name, user.Name);
      user.CreatedAt = DateTime.UtcNow;
      InsertOrUpdateUser(user);
      return true;
    }

    public bool UpdateUser(User user)
    {
      if (!Keysapce.Exists(UsersFamilyName, user.Name, UsersFamily.Name))
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
      return date.ToUniversalTime().Ticks.ToString();
    }

    private void InsertOrUpdateUser(User user)
    {
      InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Bio, user.Bio);
      InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.CreatedAt, DateToTimestamp(user.CreatedAt));
      InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.DisplayName, user.DisplayName);
      InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Email, user.Email);
      InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Location, user.Location);
      InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.Web, user.Web);
      InsertIfNotNullOrEmpty(UsersFamilyName, user.Name, UsersFamily.PasswordHash, user.PasswordHash);
    }

    private void InsertIfNotNullOrEmpty(string familyName, string key, string columnName, string value)
    {
      if (!string.IsNullOrEmpty(value))
      {
        Keysapce.Insert(familyName, key, columnName, value);
      }
    }

    private List<T> GetUserItems<T>(
      string userName,
      string joinFamily,
      Converter<AquilesColumn, T> getValueFromColumn,
      int maxItems, 
      Comparison<T> comparison)
    {
      var items = Keysapce.GetSlice(joinFamily, userName, maxItems).ConvertAll(getValueFromColumn);
      items.Sort(comparison);
      return items;
    }
  }
}