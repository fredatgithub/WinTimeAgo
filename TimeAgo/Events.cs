using System;
using System.Collections.Generic;

namespace TimeAgo
{
  internal class Events
  {
    public List<Event> ListOfEvents { get; set; }

    public Events()
    {
      ListOfEvents = new List<Event>();
    }

    public void ChangeEventDate(Event theEvent, DateTime theNewDateTime)
    {
      if (!ListOfEvents.Contains(theEvent)) return;
      ListOfEvents.Add(new Event(theEvent.Title, theNewDateTime));
      ListOfEvents.Remove(theEvent);
    }

    public void ChangeEventTitle(Event theEvent, string theNewTitle)
    {
      if (!ListOfEvents.Contains(theEvent)) return;
      ListOfEvents.Add(new Event(theNewTitle, theEvent.DateOfEvent));
      ListOfEvents.Remove(theEvent);
    }
  }
}