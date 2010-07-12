namespace JavaGeneration.Chirper.Models
{
  using System;
  using System.Collections.Generic;
  using HectorSharp;

  public class Repository : IRepository
  {
    private static readonly ColumnParent TimeLineFamilyPath = new ColumnParent("TimeLine");
    private static readonly ColumnParent UserLineFamilyPath = new ColumnParent("UserLine");
    private static readonly ColumnParent FollowingFamilyPath = new ColumnParent("Following");
    private static readonly ColumnParent FollowersFamilyPath = new ColumnParent("Followers");
    private const int MaxTimelineTweets = 150;
    private const int MaxRelatedUsers = 5000;
    private const string PublicTimeLineUser = "!PUBLIC!";

    public Repository(ICassandraClient client)
    {
      Keysapce = client.GetKeyspace("Chirper");
    }

    public IKeyspace Keysapce { get; set; }

    public IList<Tweet> GetPublicTimeLine()
    {
      return GetTimeLine(PublicTimeLineUser);
    }

    public IList<Tweet> GetTimeLine(string userName)
    {
      return GetUserItems(
        userName, TimeLineFamilyPath, c => GetTweet(c.Value), MaxTimelineTweets, (x, y) => x.Time.CompareTo(y.Time));
    }

    public IList<Tweet> GetUserLine(string userName)
    {
      return GetUserItems(
        userName, UserLineFamilyPath, c => GetTweet(c.Value), MaxTimelineTweets, (x, y) => x.Time.CompareTo(y.Time));
    }

    public IList<Following> GetFollowing(string userName)
    {
      return GetUserItems(
        userName,
        FollowingFamilyPath,
        c => new Following { User = GetUser(c.Value), Since = TimestampToDate(c.Name) },
        MaxRelatedUsers,
        (f1, f2) => f1.Since.CompareTo(f2.Since));
    }

    public IList<Follower> GetFollowers(string userName)
    {
      return GetUserItems(
        userName,
        FollowersFamilyPath,
        c => new Follower { User = GetUser(c.Value), Since = TimestampToDate(c.Name) },
        MaxRelatedUsers,
        (f1, f2) => f1.Since.CompareTo(f2.Since));
    }

    public User GetUser(string userName)
    {
      if (!Keysapce.Exists(userName, UsersFamily.Name))
      {
        return null;
      }

      return new User
        {
          Name = userName,
          PasswordHash = Keysapce.GetValueOrEmpty(userName, UsersFamily.PasswordHash),
          Bio = Keysapce.GetValueOrEmpty(userName, UsersFamily.Bio),
          DisplayName = Keysapce.GetValueOrEmpty(userName, UsersFamily.DisplayName),
          Email = Keysapce.GetValueOrEmpty(userName, UsersFamily.Email),
          Location = Keysapce.GetValueOrEmpty(userName, UsersFamily.Location),
          Web = Keysapce.GetValueOrEmpty(userName, UsersFamily.Web),
          CreatedAt = TimestampToDate(Keysapce.GetValueOrEmpty(userName, UsersFamily.CreatedAt))
        };
    }

    public Tweet GetTweet(string id)
    {
      if (!Keysapce.Exists(id, TweetsFamily.Id))
      {
        return null;
      }

      return new Tweet
        {
          Id = id,
          User = Keysapce.GetValueOrEmpty(id, TweetsFamily.User),
          InReplyToTweet = Keysapce.GetValueOrEmpty(id, TweetsFamily.InReplyToTweet),
          InReplyToUser = Keysapce.GetValueOrEmpty(id, TweetsFamily.InReplyToUser),
          Location = Keysapce.GetValueOrEmpty(id, TweetsFamily.Location),
          Text = Keysapce.GetValueOrEmpty(id, TweetsFamily.Text),
          Time = TimestampToDate(Keysapce.GetValueOrEmpty(id, TweetsFamily.Time))
        };
    }

    public void AddTweet(Tweet tweet)
    {
      tweet.Id = Guid.NewGuid().ToString();
      tweet.Time = DateTime.UtcNow;
      string tweetTimestamp = DateToTimestamp(tweet.Time);
      Keysapce.Insert(tweet.Id, TweetsFamily.Id, tweet.Id);
      Keysapce.Insert(tweet.Id, TweetsFamily.Location, tweet.Location);
      Keysapce.Insert(tweet.Id, TweetsFamily.Text, tweet.Text);
      Keysapce.Insert(tweet.Id, TweetsFamily.Time, tweetTimestamp);
      Keysapce.Insert(tweet.Id, TweetsFamily.User, tweet.User);
      if (!string.IsNullOrEmpty(tweet.InReplyToTweet) && !string.IsNullOrEmpty(tweet.InReplyToUser))
      {
        Keysapce.Insert(tweet.Id, TweetsFamily.InReplyToTweet, tweet.InReplyToTweet);
        Keysapce.Insert(tweet.Id, TweetsFamily.InReplyToUser, tweet.InReplyToUser);
      }

      Keysapce.Insert(tweet.User, new ColumnPath("UserLine", null, tweetTimestamp), tweet.Id);
      Keysapce.Insert(tweet.User, new ColumnPath("TimeLine", null, tweetTimestamp), tweet.Id);
      Keysapce.Insert(PublicTimeLineUser, new ColumnPath("TimeLine", null, tweetTimestamp), tweet.Id);

      foreach (var follower in GetFollowers(tweet.User))
      {
        Keysapce.Insert(follower.User.Name, new ColumnPath("TimeLine", null, tweetTimestamp), tweet.Id);
      }
    }

    public bool AddUser(User user)
    {
      Column nameColumn;
      if (Keysapce.TryGetColumn(user.Name, UsersFamily.Name, out nameColumn))
      {
        return false;
      }

      Keysapce.Insert(user.Name, UsersFamily.Name, user.Name);
      user.CreatedAt = DateTime.UtcNow;
      InsertOrUpdateUser(user);
      return true;
    }

    public bool UpdateUser(User user)
    {
      Column nameColumn;
      if (!Keysapce.TryGetColumn(user.Name, UsersFamily.Name, out nameColumn))
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
      Keysapce.InsertIfNotNullOrEmpty(user.Name, UsersFamily.Bio, user.Bio);
      Keysapce.InsertIfNotNullOrEmpty(user.Name, UsersFamily.CreatedAt, DateToTimestamp(user.CreatedAt));
      Keysapce.InsertIfNotNullOrEmpty(user.Name, UsersFamily.DisplayName, user.DisplayName);
      Keysapce.InsertIfNotNullOrEmpty(user.Name, UsersFamily.Email, user.Email);
      Keysapce.InsertIfNotNullOrEmpty(user.Name, UsersFamily.Location, user.Location);
      Keysapce.InsertIfNotNullOrEmpty(user.Name, UsersFamily.Web, user.Web);
      Keysapce.InsertIfNotNullOrEmpty(user.Name, UsersFamily.PasswordHash, user.PasswordHash);
    }

    private List<T> GetUserItems<T>(
      string userName, ColumnParent joinFamily, Func<Column, T> getItem, int maxItems, Comparison<T> comparison)
    {
      var result = new List<T>();
      var columns = Keysapce.GetSlice(userName, joinFamily, new SlicePredicate(new SliceRange(false, maxItems)));
      foreach (var column in columns)
      {
        result.Add(getItem(column));
      }

      result.Sort(comparison);
      return result;
    }
  }
}