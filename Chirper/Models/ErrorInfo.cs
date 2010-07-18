namespace JavaGeneration.Chirper.Models
{
  public class ErrorInfo
  {
    public ErrorInfo(string message)
    {
      Message = message;
    }

    public ErrorInfo(string message, string details)
    {
      Message = message;
      Details = details;
    }

    public string Message { get; private set; }

    public string Details { get; set; }
  }
}