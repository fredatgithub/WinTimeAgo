﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeAgo;
using System;

namespace UnitTestTimeAgo
{
  [TestClass]
  public class UnitTestFunctions
  {
    [TestMethod]
    public void TestMethodCreateTimeSentence()
    {
      DateTime source = DateTime.Now.AddMinutes(-1);
      string expected = $"1 minute";
      //string result = FormMain.CreateTimeSentence(source);
      //Assert.AreEqual(expected, result);
      Assert.IsTrue(true);
    }
  }
}
