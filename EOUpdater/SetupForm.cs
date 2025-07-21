using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EOUpdater
{
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
        }

        private void Setup_start_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("Temp");
            // FolderBrowserDialogクラスのインスタンスを作成
            using (var fbd = new FolderBrowserDialog())
            {
                Console.Write(Properties.Settings.Default.EOLocation);
                // ダイアログの説明文を設定
                fbd.Description = "処理対象のフォルダを選択してください。";

                // ルートフォルダを設定（デフォルトはDesktop）
                // fbd.RootFolder = Environment.SpecialFolder.Desktop;

                fbd.SelectedPath = @"C:\Users";

                // 新しいフォルダを作成するボタンを表示するかどうか（デフォルトはtrue）
                fbd.ShowNewFolderButton = true;

                // ダイアログを表示し、ユーザーが「OK」ボタンを押したかどうかを確認
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    // 選択されたフォルダのパスを文字列として取得
                    string selectedPath = fbd.SelectedPath;

                    Properties.Settings.Default.EOLocation = selectedPath;
                    Properties.Settings.Default.IsFirstRun = false;
                    Properties.Settings.Default.Save();

                    MessageBox.Show("セットアップが完了しました。ウィンドウを閉じてアプリケーションを再起動してください。");
                }
            }
        }
    }
}
