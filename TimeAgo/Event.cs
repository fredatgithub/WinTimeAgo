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

    public void ChangeEventTitle(Event theEvent, string theNewTitle)
    {
      theEvent.Title = theNewTitle;
    }

    public void ChangeEventDate(Event theEvent, DateTime theNewDate)
    {
      theEvent.DateOfEvent = theNewDate;
    }
  }
}