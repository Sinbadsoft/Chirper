namespace JavaGeneration.Chirper.Models
{
  using System;

  public class Follower : IComparable<Follower>, IComparable
  {
    public DateTime Since { get; set; }

    public User User { get; set; }

    public int CompareTo(Follower other)
    {
      return Since.CompareTo(other.Since);
    }

    public int CompareTo(object obj)
    {
      Follower other = obj as Follower;
      if (obj == null)
      {
        throw new ArgumentException("Object is not a Follower", "obj");
      }

      return CompareTo(other);
    }
  }
}
