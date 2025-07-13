using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO.Compression;

namespace EOUpdater
{
    public partial class EOUpdater : Form
    {
        private static readonly HttpClient client = new HttpClient();

        public EOUpdater()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // --- 設定 ---
            string fileUrl = "https://github.com/dais-k/ElectronicObserver/releases/download/"+Program.dateString+"/"+Program.dateString+".zip"; // テスト用に少し大きめのファイルURL
            string savePath = Path.Combine(Application.StartupPath, "Temp/"+Program.dateString+".zip");
            // --- 設定はここまで ---

            // UIコントロールを初期化・表示
            progressBar1.Visible = true;
            progressBar1.Value = 0;
            labelProgress.Visible = true;
            labelProgress.Text = "0%";
            button1.Enabled = false;
            ProgressStatus.Visible = true;
            ProgressStatus.Text = "ダウンロード中";

            try
            {
                // GetAsyncを使い、ヘッダーだけを先に読み込むように指定します。
                // これにより、コンテンツ本体をダウンロードする前にファイルサイズを取得できます。
                using (HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode(); // エラーがあればここで例外が発生

                    // Content-Lengthヘッダーから合計のダウンロードサイズを取得します。
                    // サーバーがこのヘッダーを送信しない場合、進捗は計算できません。
                    long? totalDownloadSize = response.Content.Headers.ContentLength;

                    // ダウンロード用のストリームを取得
                    using (Stream downloadStream = await response.Content.ReadAsStreamAsync())
                    {
                        // 書き込み先のファイルストリームを開く
                        using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            // チャンクごとに読み書きするためのバッファを用意 (8192 bytes = 8 KB)
                            var buffer = new byte[8192];
                            long totalBytesRead = 0;
                            int bytesRead;

                            // ストリームの終わりまで読み込みを繰り返す
                            while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                // ファイルにバッファの内容を書き込む
                                await fileStream.WriteAsync(buffer, 0, bytesRead);

                                // 読み込んだ合計バイト数を更新
                                totalBytesRead += bytesRead;

                                // もし合計サイズが取得できていれば、進捗を計算してUIを更新する
                                if (totalDownloadSize.HasValue)
                                {
                                    // 進捗率を計算 (0-100)
                                    int progressPercentage = (int)((double)totalBytesRead / totalDownloadSize.Value * 100);

                                    // UIコントロールを更新
                                    progressBar1.Value = progressPercentage;
                                    labelProgress.Text = $"{progressPercentage}%";
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"ファイルのダウンロードに失敗しました。\nエラー: {httpEx.Message}", "ダウンロードエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"予期せぬエラーが発生しました。\nエラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar1.Visible = false;
                labelProgress.Visible = false;
                ProgressStatus.Visible = false;
            }

            string zipPath = @"Temp/"+Program.dateString+".zip";
            string extractPath = @"Temp/" + Program.dateString ;

            progressBar1.Visible = true;
            progressBar1.Value = 0;
            labelProgress.Visible = true;
            labelProgress.Text = "0%";
            button1.Enabled = false;
            ProgressStatus.Visible = true;
            ProgressStatus.Text = "解凍中";

            // IProgress<T> を使ってUIスレッドに進捗を通知します
            var progress = new Progress<int>(value =>
            {
                progressBar1.Value = value; // プログレスバーを更新
            });

            // ラベルに進捗状況を表示する場合
            var progressText = new Progress<string>(text =>
            {
                labelProgress.Text = text;
            });

            try
            {
                await Task.Run(() => ExtractWithFileProgress(zipPath, extractPath, progress, progressText));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}");
            }

            finally
            {
                // --- 成功・失敗に関わらず、最後に必ず実行される ---
                // UIの後片付けを行う
                progressBar1.Visible = false; // プログレスバーを非表示にする
                labelProgress.Visible = false;
                ProgressStatus.Visible = false;
            }
        }

        // 解凍処理を行うメソッド
        private void ExtractWithFileProgress(string zipPath, string extractPath, IProgress<int> progress, IProgress<string> progressText)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                int totalFiles = archive.Entries.Count;
                int processedFiles = 0;

                // UIスレッドでプログレスバーの最大値を設定
                progressBar1.Invoke((Action)(() => progressBar1.Maximum = totalFiles));

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // 空のエントリ（ディレクトリなど）はスキップ
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    string destinationPath = Path.Combine(extractPath, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                    // ファイルを解凍（上書きを許可する場合は true）
                    entry.ExtractToFile(destinationPath, true);

                    processedFiles++;

                    // 進捗を通知
                    progress.Report(processedFiles);
                    progressText.Report($" {processedFiles} / {totalFiles}");
                }
            }
        }
    }
}