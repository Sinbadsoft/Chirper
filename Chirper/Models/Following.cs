namespace JavaGeneration.Chirper.Models
{
  using System;

  public class Following : IComparable<Following>, IComparable
  {
    public DateTime Since { get; set; }

    public User User { get; set; }

    public int CompareTo(Following other)
    {
      return Since.CompareTo(other.Since);
    }

    public int CompareTo(object obj)
    {
      Following other = obj as Following;
      if (obj == null)
      {
        throw new ArgumentException("Object is not a Following", "obj");
      }

      return CompareTo(other);
    }
  }
}