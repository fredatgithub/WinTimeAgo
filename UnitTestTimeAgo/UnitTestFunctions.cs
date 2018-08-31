using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TimeAgo;

namespace UnitTestTimeAgo
{
  [TestClass]
  public class UnitTestFunctions
  {
    [TestMethod]
    public void TestMethodCreateTimeSentence_add_1_minute()
    {
      DateTime source = DateTime.Now.AddMinutes(-1);
      string expected = $"1 minute";
      string result = FormMain.CreateTimeSentenceUS(source);
      Assert.IsTrue(result.StartsWith(expected));
    }

    [TestMethod]
    public void TestMethodCreateTimeSentence_add_2_minutes()
    {
      DateTime source = DateTime.Now.AddMinutes(-2);
      string expected = $"2 minutes";
      string result = FormMain.CreateTimeSentenceUS(source);
      Assert.IsTrue(result.StartsWith(expected));
    }
  }
}