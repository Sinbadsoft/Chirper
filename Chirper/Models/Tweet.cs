namespace JavaGeneration.Chirper.Models
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;

  public class Tweet
  {
    public string Id { get; set; }

    public string User { get; set; }

    public string Text { get; set; }

    public string Location { get; set; }

    public DateTime Time { get; set; }

    public string InReplyToUser { get; set; }

    public string InReplyToTweet { get; set; }
  }
}
