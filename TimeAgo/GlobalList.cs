using System.Collections.Generic;

namespace TimeAgo
{
  internal class GlobalList
  {
    public Dictionary<string, List<Event>> GlobalListOfEvents { get; set; }

    public GlobalList()
    {
      GlobalListOfEvents = new Dictionary<string, List<Event>>();
    }

    public void AddOneEvent(Event oneEvent)
    {
      if (GlobalListOfEvents.ContainsKey(oneEvent.Title))
      {
        List<Event> tmpList = GlobalListOfEvents[oneEvent.Title];
        tmpList.Add(new Event(oneEvent.Title, oneEvent.DateOfEvent));
        GlobalListOfEvents[oneEvent.Title] = tmpList;
        tmpList = null;
      }
      else
      {
        GlobalListOfEvents.Add(oneEvent.Title, new List<Event> {new Event(oneEvent.Title, oneEvent.DateOfEvent)});
      }
    }
  }
}