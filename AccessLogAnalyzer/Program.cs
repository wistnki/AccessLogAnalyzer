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
            for (int i = 2; i < args.Length; i++)
                parser.Parse(args[i]);

            parser.OutputSummaryByHour(args[0]);
            parser.OutputSummaryByHost(args[1]);

        }
    }
}