# AccessLogAnalyzer

Apache のアクセスログを集計するツールです。

## これはなに？

Apache のアクセスログを読み取り、時間帯ごとまたはアクセス元ホストごとのアクセス回数を集計します。
また、集計結果を HTML ファイルとして出力し、時間帯ごとのアクセス数のグラフを作成することもできます。

## ビルドに必要なもの

ビルドには以下のツールが必要です。

* Visual Studio 2017 以降
   * 「.NET デスクトップ開発」ワークロード
   * 「.NET Framework 4.7.2 SDK」コンポーネント
   * 「.NET Framework 4.7.2 Targeting Pack」コンポーネント
   * 「NuGet パッケージ マネジャー」コンポーネント


## ビルド方法

1. リポジトリをクローンする
1. `AccessLogAnalyzer.sln` を開く
1. Visual Studioで　`ビルド` → `ソリューションのビルド` をクリック もしくは `F5` キーを押す
   * この際、必要なパッケージが自動で読み込まれます。
1. `AccessLogAnalyzer\bin\Debug` もしくは `AccessLogAnalyzer\bin\Release` 以下に実行ファイルが作成されます。

次のファイルが生成されていれば成功です。

* `AccessLogAnalyzerTool.exe`
* `Chart.min.css`
* `Chart.min.js`
* `Oika.Libs.CuiCommandParser.dll`
* `RazorEngine.dll`
* `System.Web.Razor.dll`
* `SummaryTemplate.cshtml`

## 実行方法

コマンドプロンプトもしくは PowerShell で、以下のコマンドを実行します。

### コマンド
`AccessLogAnalyzerTool [-o SummaryFileName] [-t ByHourSummaryFileName] [-h ByHostSummaryFileName] [-s StartDate] [-e EndDate] INPUT...`

### オプション:
* `-o` : 集計結果を出力するHTMLファイル名を指定します。(省略可能)
* `-t` : 時間帯ごとの集計を出力するCSVファイル名を指定します。(省略可能)
* `-h` : ホストごとの集計を出力するCSVファイル名を指定します。(省略可能)
* `-s` : 集計対象の期間の最初の日を指定します。(省略可能)
* `-e` : 集計対象の期間の最後の日を指定します。(省略可能)
* `INPUT`: 入力するアクセスログファイルをを指定します。(複数指定可能)

-o、 -t、 -h のうち少なくとも一つは必要です。

## 出力

### 集計結果

アクセスログを集計した結果を HTML でファイルに出力します。このファイルには時間帯ごとのアクセス回数集計のグラフおよび、ホストごとのアクセス回数集計が含まれます。

### 時間帯ごとの集計

読み込んだアクセスログファイルに含まれるログのアクセス日時の全範囲、もしくは `-s` や `-e` オプションで指定した期間について、 1 時間ごとのアクセス回数を CSV でファイルに出力します。

### ホストごとの集計

ホストごとのアクセス回数を CSV でファイルに出力します。

## 依存パッケージ

* CuiCommandParser
https://github.com/oika/CuiCommandParser

* RazorEngine
https://github.com/Antaris/RazorEngine

* Chart.js
https://github.com/chartjs/Chart.js

