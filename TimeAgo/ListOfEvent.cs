using System.Collections.Generic;

namespace TimeAgo
{
  internal class ListOfEvent
  {
    public List<Event> ListOfEvents { get; set; }

    public ListOfEvent()
    {
      ListOfEvents = new List<Event>();
    }
  }
}