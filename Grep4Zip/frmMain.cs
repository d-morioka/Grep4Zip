using System;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Grep4Zip
{

    public partial class frmMain : Form
    {
        private long mlngCnt;
        private XMLINOUT mXMLINOUT;

        public frmMain()
        {
            string FName;
            Assembly mAssembly = Assembly.GetEntryAssembly();
            InitializeComponent();
            init_dGVResultGrid();

            FName = Path.ChangeExtension(mAssembly.Location,".xml");
            mXMLINOUT = new XMLINOUT(FName);
            if (mXMLINOUT.ReadData() == true)
            {
                txtKey.Text = mXMLINOUT.strKey;
                txtTGTFolder.Text = mXMLINOUT.strFolder;
                chkSubFolder.Checked = mXMLINOUT.blnSubFolder;
                txtExtension.Text = mXMLINOUT.strExtension;
            }
        }

        // *******************************************************
        // * 概要　 ... データグリッドビューの初期表示設定
        // * 引数   ... なし
        // * 戻り値 ... なし
        // * 2020.04.12 d.morioka
        // *******************************************************
        private void init_dGVResultGrid()
        {
            mlngCnt = 0;

            dGVResultGrid.Rows.Clear();
            dGVResultGrid.Columns.Clear();
            dGVResultGrid.Columns.Add("RNum", "No.");
            dGVResultGrid.Columns.Add("RFName", "ファイル名");
            dGVResultGrid.Columns.Add("RLine", "ヒット行");
            dGVResultGrid.Columns.Add("RHit", "ヒット文言");
            dGVResultGrid.RowCount = 0;
            dGVResultGrid.ReadOnly = true;
        }

        // *******************************************************
        // * 概要　 ... 実行ボタン押下イベント
        // * 引数   ... なし
        // * 戻り値 ... なし
        // * 2020.04.12 d.morioka
        // *******************************************************
        private void btnExec_Click(object sender, EventArgs e)
        {
            Boolean blnRet;

            blnRet = chkInputData();
            if (blnRet == true)
            {
                init_dGVResultGrid();
                SearchZip();
            }
            MessageBox.Show("検索完了しました",Assembly.GetExecutingAssembly().GetName().Name);
        }

        // *******************************************************
        // * 概要　 ... 指定のフォルダ内の Zipファイルを検索する
        // * 引数   ... なし
        // * 戻り値 ... なし
        // * 2020.04.12 d.morioka
        // *******************************************************
        private void SearchZip()
        {
            string[] strZipList;

            if(chkSubFolder.Checked == true)
            {
                //サブフォルダーを含め、すべての zipファイルを取得
                strZipList = Directory.GetFiles(txtTGTFolder.Text, "*.zip"
                                                            , SearchOption.AllDirectories);
            } else{
                //指定フォルダの zipファイルを取得
                strZipList = Directory.GetFiles(txtTGTFolder.Text, "*.zip"
                                                            , SearchOption.TopDirectoryOnly);
            }

            foreach (string strZipFile in strZipList)
            {
                ZipFile_OpenRead(strZipFile);
            }

        }

        // *******************************************************
        // * 概要　 ... 指定のZipファイルの中身を取得する
        // * 引数   ... Zipファイル（フルパス）
        // * 戻り値 ... なし
        // * 2020.04.12 d.morioka
        // *******************************************************
        private void ZipFile_OpenRead(string strZipFile)
        {
            using (ZipArchive zarc = ZipFile.OpenRead(strZipFile))
            {
                foreach (ZipArchiveEntry zent in zarc.Entries)
                {
                    ZipArchiveEntry entItem = zarc.GetEntry(zent.FullName);
                    if (entItem == null)
                    {
                        Console.WriteLine("ファイルが見つかりませんでした。");
                    }
                    else
                    {
                        File_OpenRead(entItem);
                    }
                }
            }
        }

        // *******************************************************
        // * 概要　 ... 指定のZipファイルの中身に指定キーが含まれるか検索する
        // * 引数   ... ファイル（zipファイルの中身）
        // * 戻り値 ... なし
        // * 2020.04.12 d.morioka
        // *******************************************************
        private void File_OpenRead(ZipArchiveEntry entItem)
        {
            long i;
            string[] arrExtensionKey;
            string strExtention = System.IO.Path.GetExtension(entItem.Name);  //指定ファイルの拡張子を取得

            arrExtensionKey = txtExtension.Text.Split(';');
            
            foreach(string strKeyExt in arrExtensionKey)
            {
                //大文字小文字を無視して比較
                if (string.Compare(strExtention, strKeyExt, true) == 0)
                {
                    using (StreamReader sr = new StreamReader(entItem.Open(),
                        System.Text.Encoding.GetEncoding("shift_jis")))
                    {
                        string s = sr.ReadToEnd();
                        string[] delchar = { "\r\n" };
                        string[] arr = s.Split(delchar, StringSplitOptions.None);
                            
                        for (i = 0; i < arr.Length; i++)
                        {
                            if (arr[i].IndexOf(txtKey.Text, StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                PutDataGrid(entItem.FullName, i + 1, arr[i]);
                            }
                        }
                    }
                    break;
                }
            }
        }

        // *******************************************************
        // * 概要　 ... フォーム入力値チェック
        // * 引数   ... なし
        // * 戻り値 ... true : 入力OK, false : 入力NG
        // * 2020.04.12 d.morioka
        // *******************************************************
        private Boolean chkInputData()
        {
            Boolean retVal;
            retVal = true;

            if (txtKey.Text == "")
            {
                MessageBox.Show("検索キーを入力してください");
                retVal = false;
            }
            if (txtTGTFolder.Text == "")
            {
                MessageBox.Show("対象フォルダーを入力してください");
                retVal = false;
            }
            if (txtExtension.Text == "")
            {
                MessageBox.Show("検索対象（拡張子）を入力してください");
                retVal = false;
            }
            if (Directory.Exists(txtTGTFolder.Text) == false)
            {
                MessageBox.Show("指定のフォルダーが見つかりません");
                retVal = false;
            }
            return retVal;
        }

        // *******************************************************
        // * 概要　 ... フォーム入力値チェック
        // * 引数   ... FName   : ファイル名列に表示する値
        // *            LineNo  : ヒット行に表示する値
        // *            LineKey : ヒット文字列に表示する値
        // * 戻り値 ... なし
        // * 2020.04.12 d.morioka
        // *******************************************************
        private void PutDataGrid(string FName,long LineNo,string LineKey)
        {
            mlngCnt += 1;

            dGVResultGrid.Rows.Add(mlngCnt,FName,LineNo,LineKey);
        }

        // *******************************************************
        // * 概要　 ... フォーム閉じるイベント
        // * 2020.05.23 d.morioka
        // *******************************************************
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 画面の設定を xmlファイルに出力
            mXMLINOUT.WriteXML(txtKey.Text, txtTGTFolder.Text, chkSubFolder.Checked, txtExtension.Text);
        }

        // *******************************************************
        // * 概要　 ... フォームリサイズイベント
        // * 2020.05.23 d.morioka
        // *******************************************************
        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            // フォームのサイズに合わせてデータグリッドのサイズを変更
            dGVResultGrid.Width = this.Width - 40;
            dGVResultGrid.Height = this.Height - 190;
        }
    }
}
