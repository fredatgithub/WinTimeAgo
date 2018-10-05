using System;
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
        GlobalListOfEvents.Add(oneEvent.Title, new List<Event> { new Event(oneEvent.Title, oneEvent.DateOfEvent) });
      }
    }

    public void RemoveOneEventList(Event theEventToBeRemoved)
    {
      if (GlobalListOfEvents.ContainsKey(theEventToBeRemoved.Title))
      {
        GlobalListOfEvents.Remove(theEventToBeRemoved.Title);
      }
    }

    public void RemoveOneSubEvent(Event theSubEventToBeRemoved)
    {
      //TODO
      //if (GlobalListOfEvents.ContainsKey(theSubEventToBeRemoved.Title))
      //{
      //  //Backup list and then remove the event to be removed
      //  GlobalListOfEvents.Remove(theSubEventToBeRemoved.Title);
      //}
    }

    public void ChangeEvent(string theKey, DateTime oldValue, DateTime newValue)
    {
      foreach (KeyValuePair<string, List<Event>> oneEventList in GlobalListOfEvents)
      {
        if (oneEventList.Key == theKey)
        {
          var tmpList = oneEventList.Value;
          foreach (Event oneEvent in tmpList)
          {
            if (oneEvent.DateOfEvent == oldValue)
            {
              var t = "debug var";
              // saving the item found
            }
          }

        }
      }
    }
  }
}