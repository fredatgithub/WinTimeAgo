using System;

namespace TimeAgo
{
  internal class Event
  {
    public string Title { get; set; }
    public DateTime DateOfEvent { get; set; }

    public Event(string title, DateTime? dateOfEvent)
    {
      Title = title;
      if (dateOfEvent == null)
      {
        DateOfEvent = DateTime.Now;
      }
      else
      {
        DateOfEvent = (DateTime)dateOfEvent;
      }
    }
  }
}