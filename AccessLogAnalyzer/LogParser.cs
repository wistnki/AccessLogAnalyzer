using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace AccessLogAnalyzer
{
    /// <summary>
    /// Apache のログを集計する機能を表します。
    /// </summary>
    public class LogParser
    {
        // 時間帯ごとのアクセス回数
        // _CountHour[Date][Hour]
        private Dictionary<DateTime, uint[]> _CountHour
            = new Dictionary<DateTime, uint[]>();

        // ホスト名ごとのアクセス回数
        private Dictionary<string, uint> _CountHost
            = new Dictionary<string, uint>();

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

            var regex = new Regex(logPattern, RegexOptions.Compiled);

            var pStart = (periodStart ?? DateTime.MinValue).Date;
            var pEnd = (periodEnd ?? DateTime.MinValue).Date;
            if (pStart > pEnd) throw new ArgumentException("periodStart must be earlier thand periodEnd.");

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
        }

        /// <summary>
        /// 時間帯ごとのアクセス回数を出力します。
        /// </summary>
        /// <param name="path">集計ファイルのパス</param>
        public void OutputSummaryByHour(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                foreach (var count in _CountHour.OrderBy(kv => kv.Key))
                {
                    for (int i = 0; i < 24; i++)
                    {
                        writer.WriteLine($@"{count.Key:yyyy/MM/dd} {i:D2},{count.Value[i]}");
                    }
                }
            }
        }

        /// <summary>
        /// ホストごとのアクセス回数を出力します。
        /// </summary>
        /// <param name="path">集計ファイルのパス</param>
        public void OutputSummaryByHost(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                foreach (var count in _CountHost.OrderByDescending(kv => kv.Value))
                {
                    writer.WriteLine($@"{count.Key},{count.Value}");
                }
            }
        }
    }
}
