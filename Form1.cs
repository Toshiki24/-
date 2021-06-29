using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace nam
{
    public partial class Form1 : Form
    {
        // フォームコンストラクタ
        public Form1()
        {
            InitializeComponent();
        }

        // 1マスの情報を格納する構造体
        private class Sell
        {
            public int[] number;          // 入力候補の数字
            public List<int> impossible;  // 絶対入りえない数字リスト
            public int group;             // グループ番号
            public int horizontal;        // 行番号
            public int vertical;          // 列番号
            public TextBox control;       // 対応するテキストボックス
        }
       
        // 定数
        // ポイント取得ライン
        private const int LINE_LOW = 3;
        private const int LINE_HIGH = 6;

        // グループ検索用ポイント
        private const int POINT_LEFT = 0;
        private const int POINT_CENTER = 1;
        private const int POINT_RIGHT = 2;

        private const int LINE_LOW_POINT = 1;
        private const int LINE_MIDDLE_POINT = 4;
        private const int LINE_HIGE_POINT = 7;

        // 入力する数字を確定することができる数
        private const int NUMBER_TO_BE_CONFIRMED = 8;

        // 入力する数字が存在しないセルのパターン(失敗)
        private const int NUMBER_ERROR = 9;

        // number初期値(未決定)
        private const int NOT_DETERMINED = 0;

        // テキストボックスの数
        private const int TEXTBOX_COUNT = 81;
        // 変数
        // マスの情報を格納する構造体のリスト
        List<Sell> list = new List<Sell>();



        /// <summary>
        /// インデックス番号からグループ番号を検索
        /// </summary>
        /// <param name="index">左上から数えた数</param>
        /// <returns>グループ番号</returns>
        private int SearchGroup(int index)
        {
            int point = 0;

            Double i = index / 9.00;

            // 何行目のグループかによってポイントを付ける
            if (i <= LINE_LOW)
                point = LINE_LOW_POINT;
            else if (i <= LINE_HIGH)
                point = LINE_MIDDLE_POINT;
            else
                point = LINE_HIGE_POINT;

            // 何列目のグループかによってポイントを付ける
            if (index % 9 != 0)
            {
                int j = index % 9;

                if (j <= LINE_LOW)
                    point += POINT_LEFT;
                else if (j <= LINE_HIGH)
                    point += POINT_CENTER;
                else
                    point += POINT_RIGHT;
            }
            else
            {
                // 9で割り切れたら右の列
                point += POINT_RIGHT;
            }
            return point;
        }

        /// <summary>
        /// impossibleの更新
        /// </summary>
        private bool UpdateImpossible()
        {
            // 初めにすべてのimpossibleを初期化
            list.ForEach(x => x.impossible.Clear());

            foreach (Sell sell in list)
            {
                if (!InfomationSharing(sell))
                    return false;
            }
            // 最後まで正常に処理されたらtrue
            return true;
        }

        /// <summary>
        /// 絶対に入らない数リストの更新
        /// </summary>
        /// <param name="sell">情報共有するセル</param>
        private bool InfomationSharing(Sell sell)
        {
            // 現在処理sellのnumber取得
            int value = sell.number.Last();

            // 処理中の番号が無い場合は正常終了
            if (value == NOT_DETERMINED)
                return true;

            foreach (Sell s in list)
            {
                // 同じグループの場合
                if (s.group == sell.group && !s.impossible.Contains(value))
                    s.impossible.Add(value);

                // 同じ行の場合
                if (s.horizontal == sell.horizontal && !s.impossible.Contains(value))
                    s.impossible.Add(value);

                // 同じ列の場合
                if (s.vertical == sell.vertical && !s.impossible.Contains(value))
                    s.impossible.Add(value);

                // 追加後にリストが9以上(以上パターン)になった場合はfalseを返す
                if (s.impossible.Count == NUMBER_ERROR && s.number.Last() == NOT_DETERMINED)
                    return false;
            }
            // 最後まで正常に処理された場合
            return true;
        }

        /// <summary>
        /// 数字の初期
        /// </summary>
        /// <returns>チェッククリア：true</returns>
        private bool ValidationCheck()
        {
            // 何も入力されていない場合
            if (list.All(x => String.IsNullOrWhiteSpace(x.control.Text)))
                return false;

            foreach (Sell s in list)
            {
                // 空なら次のセルへ
                if (String.IsNullOrWhiteSpace(s.control.Text))
                    continue;

                // 列に同一数字がある場合
                if (list.FindAll(x => s.vertical == x.vertical &&
                    s.control.Text == x.control.Text).Count() > 1)
                    return false;

                // 行に同一数字がある場合
                if (list.FindAll(x => s.horizontal == x.horizontal &&
                    s.control.Text == x.control.Text).Count() > 1)
                    return false;

                // グループに同一数字がある場合
                if (list.FindAll(x => s.group == x.group &&
                    s.control.Text == x.control.Text).Count() > 1)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// numberへの書き込み
        /// </summary>
        /// <param name="sell">numberを書き込みたいSell</param>
        private void WritingSell(Sell sell)
        {
            // どの文字を書き込むか検索
            for (int i = 1; i <= 9; i++)
            {
                // 配列内に存在しない数字を発見した場合は書き込む
                if (!sell.impossible.Contains(i))
                {
                    // 最後尾を書き換え
                    sell.number[sell.number.Length - 1] = i;
                    // 書き変えた時点で終了
                    return;
                }
            }
        }

        /// <summary>
        /// 検索アルゴリズム本体
        /// </summary>
        private bool SearchMain()
        {
            // セルの数字が全部確定するまで繰り返す
            while (list.Any(x => x.number.Last() == 0))
            {
                // 候補数が一番小さいセル取得
                Sell processingSell = new Sell();
                for (int i = NUMBER_TO_BE_CONFIRMED; i > 0; i--)
                {
                    processingSell = list.FindAll(x => x.number.Last() == 0)
                    .Find(x => x.impossible.Count() == i);

                    // 上で取得できた場合
                    if (processingSell != null)
                        break;
                }

                // 入る数字が確定している場合
                if (processingSell.impossible.Count() == NUMBER_TO_BE_CONFIRMED)
                {
                    // ナンバーに登録
                    WritingSell(processingSell);

                    if (!InfomationSharing(processingSell))
                    {
                        // 書き込みの情報共有結果でエラーだった場合ありえないパターン
                        // 処理中のnumberを削除する
                        list.ForEach(x => Array.Resize(ref x.number, x.number.Length - 1));
                        // 1層上の処理に戻る
                        return false;
                    }
                }
                else
                {
                    // 入る数字が確定していない場合

                    //候補リストの作成
                    List<int> candidateList = new List<int>();

                    for (int i = 1; i <= 9; i++)
                    {
                        if (!processingSell.impossible.Contains(i))
                            candidateList.Add(i);
                    }

                    // 候補リスト分処理を繰り返す
                    foreach (int num in candidateList)
                    {
                        // ナンバーの要素数を取得()
                        int index = processingSell.number.Length;

                        // 候補となった数値を使用してnumber配列要素を追加
                        foreach (Sell sell in list)
                        {
                            // numberのリサイズ
                            Array.Resize(ref sell.number, sell.number.Length + 1);

                            // 現在処理中のセルのみ候補数を格納
                            if (sell.control.Name == processingSell.control.Name)
                                processingSell.number[index] = num;
                            else
                                // 他のセルには一つまえの数値を入れる
                                sell.number[index] = sell.number[index - 1];
                        }

                        // 仮の数字を入れて整合性が取れていない場合は次の候補を試す
                        if (!UpdateImpossible())
                        {
                            // 処理中のnumberを削除する
                            list.ForEach(x => Array.Resize(ref x.number, x.number.Length - 1));
                            continue;
                        }
                        // 再帰呼び出し
                        if (SearchMain())
                            // trueが返ってきた場合はnumberそのまま
                            return true;
                    }
                    // ここまで来たら現在のnumberは失敗確定の為削除
                    list.ForEach(x => Array.Resize(ref x.number, x.number.Length - 1));

                    // 1階層上に戻る
                    return false;
                }
            }
            // 全て完成した場合はtrueを返す
            return true;
        }

        /// <summary>
        /// 実行ボタン押下イベント
        /// </summary>
        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                list.Clear();

                // 1マスずつ登録していく
                for (int i = 1; i <= 81; i++)
                {
                    // コントロールの取得
                    string conName = "t" + i.ToString();
                    Control[] controls = this.Controls.Find(conName, true);

                    Sell sell = new Sell();

                    sell.number = new int[1];
                    sell.number[0] = 0;
                    sell.group = SearchGroup(i);
                    sell.impossible = new List<int>();
                    sell.horizontal = (i - 1) / 9 + 1;
                    sell.vertical = i % 9 != 0 ? i % 9 : 9;
                    sell.control = (TextBox)controls[0];

                    if (int.TryParse(sell.control.Text, out int num))
                        sell.number[0] = num;

                    list.Add(sell);
                }

                // バリデーションチェック
                if (!ValidationCheck())
                {
                    MessageBox.Show("縦、横、同一グループ内に同じ数字が無いか確認してください。");
                    return;
                }

                // 絶対に入らない数リストの更新
                if (!UpdateImpossible())
                {
                    // ありえないリストの数が9つのセルを見つけた場合
                    MessageBox.Show("答えが存在しない問題です");
                    return;
                }

                //　メイン検索アルゴリズム
                if (SearchMain())
                {
                    // number変数の最後尾を対応するテキストボックスに格納する
                    list.ForEach(x => x.control.Text = x.number.Last().ToString());
                    MessageBox.Show("完了しました。");
                }
                else
                {
                    MessageBox.Show("答えが存在しない問題です");
                }
            }
            catch
            {
                MessageBox.Show("例外");
            }
        }

        /// <summary>
        /// 削除ボタン押下イベント
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 1; i <= 81; i++)
                {
                    string conName = "t" + i.ToString();
                    Control[] con = this.Controls.Find(conName, true);
                    con[0].Text = "";
                }
            }
            catch
            {
                MessageBox.Show("例外");
            }
        }

        /// <summary>
        /// テキストボックスの入力時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextChanged(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (!String.IsNullOrEmpty(textbox.Text))
            {
                // 次のコントロールの取得
                int idx = int.Parse(textbox.Name.Substring(1));

                // 最後のテキストボックスの場合はボタンをフォーカス
                if (idx == TEXTBOX_COUNT)
                {
                    btnRun.Focus();
                }
                else
                {
                    string next = "t" + (idx + 1);
                    Control control = this.Controls.Find(next, true).First();
                    control.Focus();
                }
            }

        }
    }
}
