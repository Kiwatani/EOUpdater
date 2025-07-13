using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using System.Net.Http;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace EOUpdater
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>

        public static string dateString = "";
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 初回起動かどうかを判定
            if (Properties.Settings.Default.IsFirstRun)
            {
                // ここに初回起動時にのみ実行したい処理を記述します。
                // 例えば、初回セットアップ用のフォームを表示します。
                /*using (SetupForm setupForm = new SetupForm())
                {
                    // セットアップフォームをモーダルで表示
                    if (setupForm.ShowDialog() == DialogResult.OK)
                    {
                        // セットアップが正常に完了した場合の処理
                    }
                    else
                    {
                        // セットアップがキャンセルされた場合はアプリケーションを終了する
                        return;
                    }
                }*/

                // 初回起動フラグをfalseに更新して保存
                Properties.Settings.Default.IsFirstRun = false;
                Properties.Settings.Default.Save();
            }

            using (WebClient client = new WebClient())
            {
                // テキストファイルのURL
                string url = "https://raw.githubusercontent.com/dais-k/ElectronicObserver/develop/ElectronicObserver/version.txt";

                try
                {
                    // 指定したURLからリソースを文字列としてダウンロードします。
                    string fileContent = client.DownloadString(url);

                    string[] lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    if (lines.Length >= 2)
                    {
                        // 2行目の内容をstring変数に格納する
                        dateString = lines[1];

                        // 結果をコンソールに出力して確認
                        Console.WriteLine("ファイルから読み込んだ日付文字列:");
                        Console.WriteLine(dateString);
                    }
                    else
                    {
                        Console.WriteLine("エラー: ファイルに2行以上のデータがありません。");
                    }
                }
                catch (WebException e)
                {
                    Console.WriteLine($"エラーが発生しました: {e.Message}");
                }
            }



                Application.Run(new EOUpdater());
        }
    }
}
