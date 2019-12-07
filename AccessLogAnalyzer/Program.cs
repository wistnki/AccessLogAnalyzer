using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace AccessLogAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new LogParser();
            parser.Parse(args[0], new DateTime(2005, 4, 18), new DateTime(2005, 4, 20));
            parser.OutputSummaryByHour(args[1]);
            parser.OutputSummaryByHost(args[2]);
        }
    }
}