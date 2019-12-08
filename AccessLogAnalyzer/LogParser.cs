using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

using RazorEngine;
using RazorEngine.Templating;

namespace AccessLogAnalyzer
{
    /// <summary>
    /// Apache のログを集計する機能を表します。
    /// </summary>
    public class LogParser
    {
        public ReadOnlyDictionary<string, uint> CountByHour { get; private set; }

        public ReadOnlyDictionary<string, uint> CountByHost { get; private set; }

        /// <summary>
        /// ログファイルを読み取って集計します。
        /// 複数のファイルから読み込む場合は、同一の<see cref="LogParser"/>インスタンスで
        /// このメソッドをファイルごとに呼び出します。
        /// </summary>
        /// <param name="path">ログファイルのパス</param>
        /// <param name="periodStart">集計対象の期間の最初の日</param>
        /// <param name="periodEnd">集計対象の期間の最後の日</param>
        public void Parse(string path, DateTime? periodStart = null, DateTime? periodEnd = null)
        {
            const string logPattern = @"(.*?)\s.*?\s\[(.*)\]\s";
            const string dateTimeFormat = @"dd\/MMM\/yyyy\:HH\:mm\:ss zzz";

            // 時間帯ごとのアクセス回数(_CountHour[Date][Hour])
            Dictionary<DateTime, uint[]> _CountHour = new Dictionary<DateTime, uint[]>();

            // ホスト名ごとのアクセス回数
            Dictionary<string, uint> _CountHost = new Dictionary<string, uint>();

            var regex = new Regex(logPattern, RegexOptions.Compiled);

            var pStart = (periodStart ?? DateTime.MinValue).Date;
            var pEnd = (periodEnd ?? DateTime.MaxValue).Date;
            if (pStart > pEnd) throw new ArgumentException("periodStart must be earlier thand periodEnd.");

            try
            {
                var rawLogLines = File.ReadLines(path);

                foreach (var line in rawLogLines)
                {
                    // 読み込み
                    var match = regex.Match(line);
                    var host = match.Groups[1].Value;
                    var dateTime = DateTime.ParseExact(match.Groups[2].Value, dateTimeFormat, DateTimeFormatInfo.InvariantInfo);

                    // 集計期間内かをチェック
                    if (dateTime.Date < pStart.Date || pEnd.Date < dateTime.Date) continue;

                    // 時間帯ごとの集計
                    if (!_CountHour.ContainsKey(dateTime.Date))
                        _CountHour[dateTime.Date] = new uint[24];
                    _CountHour[dateTime.Date][dateTime.Hour]++;

                    // ホストごとの集計
                    if (!_CountHost.ContainsKey(host))
                        _CountHost[host] = 0;
                    _CountHost[host]++;
                }

                // 集計結果をプロパティに格納
                var dicDate = new Dictionary<string, uint>();
                foreach (var kv in _CountHour.OrderBy(k => k.Key))
                {
                    for (int i = 0; i < kv.Value.Length; i++)
                    {
                        dicDate.Add($@"{kv.Key:yyyy/MM/dd} {i:D2}", kv.Value[i]);
                    }
                }
                CountByHour = new ReadOnlyDictionary<string, uint>(dicDate);

                CountByHost = new ReadOnlyDictionary<string, uint>(_CountHost
                    .OrderByDescending(kv => kv.Value)
                    .ToDictionary(kv => kv.Key, kv => kv.Value));

            }
            catch (Exception e)
            {
                Console.WriteLine($"{path} の読み込み時にエラーが発生しました。");
                return;
            }

        }

        /// <summary>
        /// 集計結果をHTMLで出力します。
        /// </summary>
        /// <param name="path">集計ファイルのパス</param>
        /// <param name="templatePath">テンプレートファイルのパス</param>
        public void OutputSummary(string path, string templatePath)
        {
            var template = File.ReadAllText(templatePath);
            string html = Engine.Razor.RunCompile(template, "templateKey", null, this);
            File.WriteAllText(path, html);
        }

        /// <summary>
        /// 時間帯ごとのアクセス回数を出力します。
        /// </summary>
        /// <param name="path">集計ファイルのパス</param>
        public void OutputSummaryByHour(string path)
        {
            try
            {
                using (var writer = new StreamWriter(path))
                {
                    foreach (var count in CountByHour)
                    {
                        writer.WriteLine($@"{count.Key},{count.Value}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{path} の書き込み時にエラーが発生しました。\nファイルが使用中であるか書き込み禁止になっている可能性があります。");
            }
        }

        /// <summary>
        /// ホストごとのアクセス回数を出力します。
        /// </summary>
        /// <param name="path">集計ファイルのパス</param>
        public void OutputSummaryByHost(string path)
        {
            try
            {
                using (var writer = new StreamWriter(path))
                {
                    foreach (var count in CountByHost)
                    {
                        writer.WriteLine($@"{count.Key},{count.Value}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{path} の書き込み時にエラーが発生しました。\nファイルが使用中であるか書き込み禁止になっている可能性があります。");
            }
        }
    }
}
