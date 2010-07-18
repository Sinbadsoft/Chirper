namespace JavaGeneration.Chirper.Models
{
  using System.Collections.Generic;

  public interface IRepository
  {
    User GetUser(string userName);

    IList<Tweet> GetPublicTimeLine();

    IList<Tweet> GetTimeLine(string userName);

    IList<Tweet> GetUserLine(string userName);

    IList<Following> GetFollowing(string userName);

    IList<Follower> GetFollowers(string userName);

    bool Follow(string follower, string following);

    bool UnFollow(string follower, string following);

    Tweet GetTweet(string id);

    void AddTweet(Tweet tweet);

    bool AddUser(User user);

    bool UpdateUser(User user);
  }
}
