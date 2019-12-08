using System;
using System.IO;
using Oika.Libs.CuiCommandParser;

namespace AccessLogAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            // コマンドライン引数の登録
            var cmdParser = new CommandParser();
            cmdParser.RegisterOption(new CommandOption('t', null, null, "時間帯ごとの集計を出力するファイル名を指定します。", CommandOptionKind.NeedsValue));
            cmdParser.RegisterOption(new CommandOption('h', null, null, "ホストごとの集計を出力するファイル名を指定します。", CommandOptionKind.NeedsValue));
            cmdParser.RegisterOption(new CommandOption('s', null, null, "集計対象の期間の最初の日を指定します。", CommandOptionKind.NeedsValue));
            cmdParser.RegisterOption(new CommandOption('e', null, null, "集計対象の期間の最後の日を指定します。", CommandOptionKind.NeedsValue));
            var parsedCmd = cmdParser.Parse(args);

            // コマンドライン引数のチェック
            if (parsedCmd == null ||
                (!parsedCmd.HasOption('t') && !parsedCmd.HasOption('h')) ||
                parsedCmd.CommandParameters.Count == 0)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
                var usageBuilder = cmdParser.NewUsageBuilder(assemblyName);
                OutputUsage(usageBuilder);
                return;
            }

            // 集計対象の期間の読み取り
            var pStart = default(DateTime?);
            var pEnd = default(DateTime?);
            if (parsedCmd.HasOption('s'))
            {
                if (DateTime.TryParse(parsedCmd.GetOptionValue('s'), out var date))
                {
                    pStart = date;
                }
                else
                {
                    Console.WriteLine("集計対象の期間の最初の日を指定の指定が不正です。");
                    return;
                }
            }
            if (parsedCmd.HasOption('e'))
            {
                if (DateTime.TryParse(parsedCmd.GetOptionValue('e'), out var date))
                {
                    pEnd = date;
                }
                else
                {
                    Console.WriteLine("集計対象の期間の最後の日を指定の指定が不正です。");
                    return;
                }
            }

            // ログを解析
            var logParser = new LogParser();
            foreach (var input in parsedCmd.CommandParameters)
                logParser.Parse(input);

            // 結果の出力
            if (parsedCmd.HasOption('t')) logParser.OutputSummaryByHour(parsedCmd.GetOptionValue('t'));
            if (parsedCmd.HasOption('h')) logParser.OutputSummaryByHost(parsedCmd.GetOptionValue('h'));

            return;
        }

        /// <summary>
        /// Usage を生成して出力します。
        /// </summary>
        /// <param name="usageBuilder">CommandUsageBuilder</param>
        private static void OutputUsage(CommandUsageBuilder usageBuilder)
        {
            usageBuilder.Summary = "Apache のアクセスログを集計して、時間帯ごともしくはホストごとのアクセス数を出力します。";
            usageBuilder.AddUseCase(usageBuilder.NewUseCase()
                .AddArg(usageBuilder.NewUseCaseArg("-t").Value("ByHourSummaryFileName"))
                .AddArg(usageBuilder.NewUseCaseArg("-h").Value("ByHostSummaryFileName"))
                .AddArg(usageBuilder.NewUseCaseArg("-s").Value("StartDate").AsOptional())
                .AddArg(usageBuilder.NewUseCaseArg("-e").Value("EndDate").AsOptional())
                .AddArg(usageBuilder.NewUseCaseArg("INPUT").AsMultiple()));

            Console.WriteLine(usageBuilder.ToString());
        }
    }
}