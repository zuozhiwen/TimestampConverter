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
        private static Regex TimestampRegex = new Regex(@"(?<=\s|,|^)(\d{13}|\d{10})(?=\s|,|$)", RegexOptions.Compiled);
        private static readonly Regex DatetimeStringRegex = new Regex(@"\d{4}-\d{1,2}-\d{1,2}((\s|T)\d{1,2}:\d{1,2}(:\d{2})?)?", RegexOptions.Compiled);
        private TimeZoneInfo lastCalcTimeZone;

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

            // 初始化时区下拉框
            foreach (var item in TimeZoneInfo.GetSystemTimeZones())
            {
                comboBox1.Items.Add(item);
            }

            comboBox1.SelectedItem = comboBox1.Items.Cast<TimeZoneInfo>().Where(o=>o.DisplayName.Contains("北京")).First();
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
                    DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(matchValue.Value), comboBox1.SelectedItem as TimeZoneInfo ?? TimeZoneInfo.Local);
                    var finalDate = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
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
            if (dt == null)
            {
                return;
            }

            richTextBox1.SelectionStart = richTextBox1.TextLength;
            if (richTextBox1.Text.Length != 0 && !richTextBox1.Text.EndsWith("\n"))
            {
                richTextBox1.AppendText("\n");
            }

            // 时区变化
            if (lastCalcTimeZone != comboBox1.SelectedItem)
            {
                richTextBox1.AppendText("\n时区：" + comboBox1.SelectedItem.ToString() + "\n");
            }
            lastCalcTimeZone = comboBox1.SelectedItem as TimeZoneInfo;

            // 计算日期
            var localDt = TimeZoneInfo.ConvertTime(dt, comboBox1.SelectedItem as TimeZoneInfo ?? TimeZoneInfo.Local);
            if (localDt == null)
            {
                return;
            }

            richTextBox1.AppendText($"{localDt:yyyy-MM-dd HH:mm:ss} -- {(int)(dt - OriginDate).TotalSeconds}\n");
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

        private void button2_Click(object sender, EventArgs e)
        {
            var clipText = Clipboard.GetText(TextDataFormat.UnicodeText);
            var matchCollection = DatetimeStringRegex.Matches(clipText);
            if(matchCollection.Count == 1)
            {
                var match = matchCollection.Cast<Match>().First();
                var dateTime = DateTime.Parse(match.Value);
                var sql = $"{(long)(dateTime.Date - OriginDate).TotalMilliseconds} <= ts and ts < {(long)(dateTime.Date.AddDays(1) - OriginDate).TotalMilliseconds}";
                Clipboard.SetText(sql, TextDataFormat.UnicodeText);
                richTextBox1.AppendText(sql + "\n");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var clipText = Clipboard.GetText(TextDataFormat.UnicodeText);
            var matchCollection = DatetimeStringRegex.Matches(clipText);
            if (matchCollection.Count == 1)
            {
                var match = matchCollection.Cast<Match>().First();
                var dateTime = DateTime.Parse(match.Value);
                var mongoSql = "\"timestamp\": { $gte: NumberInt(" + (int)(dateTime.Date - OriginDate).TotalSeconds + "), $lt: NumberInt(" + (int)(dateTime.Date.AddDays(1) - OriginDate).TotalSeconds + ") }";
                Clipboard.SetText(mongoSql, TextDataFormat.UnicodeText);
                richTextBox1.AppendText(mongoSql + "\n");
            }
        }
    }
}
