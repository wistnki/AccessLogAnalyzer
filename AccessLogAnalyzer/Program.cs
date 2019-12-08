using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Oika.Libs.CuiCommandParser;

namespace AccessLogAnalyzer
{
    class Program
    {
        static int Main(string[] args)
        {
            const string SummaryTemplatePath = "SummaryTemplate.cshtml";

            // RazorEngine用に別ドメインで実行
            // c.f.: https://antaris.github.io/RazorEngine/index.html#Temporary-files
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                // RazorEngine cannot clean up from the default appdomain...
                //Console.WriteLine("Switching to secound AppDomain, for RazorEngine...");
                AppDomainSetup adSetup = new AppDomainSetup();
                adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                var current = AppDomain.CurrentDomain;
                // You only need to add strongnames when your appdomain is not a full trust environment.
                var strongNames = new StrongName[0];

                var domain = AppDomain.CreateDomain(
                    "MyMainDomain", null,
                    current.SetupInformation, new PermissionSet(PermissionState.Unrestricted),
                    strongNames);
                var exitCode = domain.ExecuteAssembly(Assembly.GetExecutingAssembly().Location, args);
                // RazorEngine will cleanup. 
                AppDomain.Unload(domain);
                return exitCode;
            }

            // コマンドライン引数の登録
            var cmdParser = new CommandParser();
            cmdParser.RegisterOption(new CommandOption('o', null, null, "集計結果を出力するHTMLファイル名を指定します。", CommandOptionKind.NeedsValue));
            cmdParser.RegisterOption(new CommandOption('t', null, null, "時間帯ごとの集計を出力するCSVファイル名を指定します。", CommandOptionKind.NeedsValue));
            cmdParser.RegisterOption(new CommandOption('h', null, null, "ホストごとの集計を出力するCSVファイル名を指定します。", CommandOptionKind.NeedsValue));
            cmdParser.RegisterOption(new CommandOption('s', null, null, "集計対象の期間の最初の日を指定します。", CommandOptionKind.NeedsValue));
            cmdParser.RegisterOption(new CommandOption('e', null, null, "集計対象の期間の最後の日を指定します。", CommandOptionKind.NeedsValue));
            var parsedCmd = cmdParser.Parse(args);

            // コマンドライン引数のチェック
            if (parsedCmd == null ||
                (!parsedCmd.HasOption('o') && !parsedCmd.HasOption('t') && !parsedCmd.HasOption('h')) ||
                parsedCmd.CommandParameters.Count == 0)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
                var usageBuilder = cmdParser.NewUsageBuilder(assemblyName);
                OutputUsage(usageBuilder);
                return 1;
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
                    return 1;
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
                    return 1;
                }
            }

            // ログを解析
            var logParser = new LogParser();
            logParser.Parse(parsedCmd.CommandParameters, pStart, pEnd);

            // 結果の出力
            if (parsedCmd.HasOption('o')) logParser.OutputSummary(parsedCmd.GetOptionValue('o'), SummaryTemplatePath);
            if (parsedCmd.HasOption('t')) logParser.OutputSummaryByHour(parsedCmd.GetOptionValue('t'));
            if (parsedCmd.HasOption('h')) logParser.OutputSummaryByHost(parsedCmd.GetOptionValue('h'));

            return 0;
        }

        /// <summary>
        /// Usage を生成して出力します。
        /// </summary>
        /// <param name="usageBuilder">CommandUsageBuilder</param>
        private static void OutputUsage(CommandUsageBuilder usageBuilder)
        {
            usageBuilder.Summary = "Apache のアクセスログを集計して、時間帯ごともしくはホストごとのアクセス数を出力します。";
            usageBuilder.AddUseCase(usageBuilder.NewUseCase()
                .AddArg(usageBuilder.NewUseCaseArg("-o").Value("SummaryFileName").AsOptional())
                .AddArg(usageBuilder.NewUseCaseArg("-t").Value("ByHourSummaryFileName").AsOptional())
                .AddArg(usageBuilder.NewUseCaseArg("-h").Value("ByHostSummaryFileName").AsOptional())
                .AddArg(usageBuilder.NewUseCaseArg("-s").Value("StartDate").AsOptional())
                .AddArg(usageBuilder.NewUseCaseArg("-e").Value("EndDate").AsOptional())
                .AddArg(usageBuilder.NewUseCaseArg("INPUT").AsMultiple()));

            Console.WriteLine(usageBuilder.ToString());
        }
    }
}