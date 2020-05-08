using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimestampConverter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static readonly DateTime OriginDate = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        private static Regex TimestampRegex = new Regex(@"(?<=\s)(\d{13}|\d{10})(?=\s|,|$)", RegexOptions.Compiled);
        private static readonly Regex DatetimeStringRegex = new Regex(@"\d{4}-\d{1,2}-\d{1,2}((\s|T)\d{1,2}:\d{1,2}(:\d{2})?)?", RegexOptions.Compiled);

        private void Form1_Load(object sender, EventArgs e)
        {
            var dateList = new List<DateTime>()
            {
                DateTime.Today.AddYears(-1),
                DateTime.Today.AddMonths(-1),
                DateTime.Today.AddDays(-2),
                DateTime.Today.AddDays(-1),
                DateTime.Today,
                DateTime.Today.AddDays(1)
            };

            foreach (var dt in dateList)
            {
                AppendToDisplay(dt);
            }

            richTextBox1.AppendText("\n");
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var clipText = Clipboard.GetText(TextDataFormat.UnicodeText);
            MatchCollection matchCollection;
            bool matched = false;

            if ((matchCollection = TimestampRegex.Matches(clipText))?.Count > 0)
            {
                foreach (string matchValue in matchCollection.Cast<Match>().Select(o => o.Value).OrderBy(o => o))
                {
                    double timestamp = 0;
                    if (matchValue.Length == 13)
                    {
                        timestamp = Convert.ToDouble(matchValue) / 1000;
                    }
                    else
                    {
                        timestamp = Convert.ToDouble(matchValue);
                    }

                    var finalDate = OriginDate.AddSeconds(timestamp);
                    AppendToDisplay(finalDate);
                }

                matched = true;
            }

            if ((matchCollection = DatetimeStringRegex.Matches(clipText))?.Count > 0)
            {
                foreach (Match matchValue in matchCollection)
                {
                    var finalDate = DateTime.Parse(matchValue.Value);
                    AppendToDisplay(finalDate);
                    if (finalDate.TimeOfDay.TotalSeconds == 0 && matchCollection.Count == 1)
                    {
                        AppendToDisplay(finalDate.AddDays(1));
                    }
                }

                matched = true;
            }

            if (matched == false)
            {
                AppendToDisplay(DateTime.Now);
            }
        }

        private void AppendToDisplay(DateTime dt)
        {
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            if (richTextBox1.Text.Length != 0 && !richTextBox1.Text.EndsWith("\n"))
            {
                richTextBox1.AppendText("\n");
            }

            richTextBox1.AppendText($"{dt.ToString("yyyy-MM-dd HH:mm:ss")} -- {(int)(dt - OriginDate).TotalSeconds}\n");
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                // 文本中查找
                TimestampRegex = new Regex(@"(?<=\D|^)(\d{13}|\d{10})(?=\D|$)", RegexOptions.Compiled);
            }
            else
            {
                // 查找独立的时间戳文本
                TimestampRegex = new Regex(@"(?<=\s|,|^)(\d{13}|\d{10})(?=\s|,|$)", RegexOptions.Compiled);
            }
        }
    }
}
