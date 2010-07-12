namespace JavaGeneration.Chirper.Models
{
  using HectorSharp;

  public static class TweetsFamily
  {
    public const string NAME = "Tweets";
    public static readonly ColumnPath Id = new ColumnPath(NAME, null, "Id");
    public static readonly ColumnPath User = new ColumnPath(NAME, null, "User");
    public static readonly ColumnPath InReplyToUser = new ColumnPath(NAME, null, "InReplyToUser");
    public static readonly ColumnPath InReplyToTweet = new ColumnPath(NAME, null, "InReplyToTweet");
    public static readonly ColumnPath Text = new ColumnPath(NAME, null, "Text");
    public static readonly ColumnPath Location = new ColumnPath(NAME, null, "Location");
    public static readonly ColumnPath Time = new ColumnPath(NAME, null, "Time");
  }
}