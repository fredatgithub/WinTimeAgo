using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeAgo;

namespace UnitTestTimeAgo
{
  [TestClass]
  public class UnitTestFunctions
  {
    [TestMethod]
    public void TestMethodCreateTimeSentence()
    {
      DateTime source = new DateTime(2018, 7, 10);
      string expected = $"20 hours 58 minutes 49 seconds 272 milliseconds";
      string result = FormMain.CreateTimeSentence(source);
      Assert.AreEqual(expected, result);
    }
  }
}
