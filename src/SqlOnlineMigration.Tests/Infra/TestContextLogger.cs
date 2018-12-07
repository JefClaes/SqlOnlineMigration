using System;
using NUnit.Framework;

namespace SqlOnlineMigration.Tests.Infra
{
    public class TestContextLogger : ILogger
    {
        public void Debug(string msg)
        {
            TestContext.Progress.WriteLine($"[{DateTime.Now}] {msg}");
        }
    }
}
