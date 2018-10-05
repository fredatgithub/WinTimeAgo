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
  }
}