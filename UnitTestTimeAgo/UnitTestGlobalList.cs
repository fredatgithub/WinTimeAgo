using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeAgo;

namespace UnitTestTimeAgo
{
  [TestClass]
  public class UnitTestGlobalList
  {
    [TestMethod]
    public void TestMethod_1()
    {
      DateTime source = DateTime.Now.AddMinutes(-2);
      const string expected = "2 minutes";
      string result = FormMain.CreateTimeSentenceUs(source);
      Assert.IsTrue(result.StartsWith(expected));
    }
  }
}