namespace JavaGeneration.Chirper.Models
{
  using System;

  public class User
  {
    public string Name { get; set; }

    public string PasswordHash { get; set; }

    public string DisplayName { get; set; }

    public string Location { get; set; }

    public string Web { get; set; }

    public string Bio { get; set; }

    public string Email { get; set; }

    public DateTime CreatedAt { get; set; }
  }
}