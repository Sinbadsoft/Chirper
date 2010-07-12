namespace JavaGeneration.Chirper.Models
{
  using HectorSharp;

  public static class UsersFamily
  {
    public const string NAME = "Users";
    public static readonly ColumnPath PasswordHash = new ColumnPath(NAME, null, "PasswordHash");
    public static readonly ColumnPath Name = new ColumnPath(NAME, null, "Name");
    public static readonly ColumnPath Email = new ColumnPath(NAME, null, "Email");
    public static readonly ColumnPath Bio = new ColumnPath(NAME, null, "Bio");
    public static readonly ColumnPath DisplayName = new ColumnPath(NAME, null, "DisplayName");
    public static readonly ColumnPath Location = new ColumnPath(NAME, null, "Location");
    public static readonly ColumnPath Web = new ColumnPath(NAME, null, "Web");
    public static readonly ColumnPath CreatedAt = new ColumnPath(NAME, null, "CreatedAt");
  }
}