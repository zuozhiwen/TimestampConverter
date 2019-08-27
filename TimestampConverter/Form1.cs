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
        private static readonly Regex TimestampRegex = new Regex(@"\d{10}|\d{13}", RegexOptions.Compiled);
        private static readonly Regex DatetimeStringRegex = new Regex(@"\d{4}-\d{1,2}-\d{1,2}((\s|T)\d{1,2}:\d{1,2}(:\d{2})?)?");

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

            foreach(var dt in dateList)
            {
                AppendToDisplay(dt);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var clipText = Clipboard.GetText(TextDataFormat.UnicodeText);
            MatchCollection matchCollection = null;
            if ((matchCollection = TimestampRegex.Matches(clipText))?.Count > 0)
            {
                foreach (Match matchValue in matchCollection)
                {
                    double timestamp = 0;
                    if (clipText.Length == 13)
                    {
                        timestamp = Convert.ToDouble(matchValue.Value) / 1000;
                    }
                    else
                    {
                        timestamp = Convert.ToDouble(matchValue.Value);
                    }

                    var finalDate = OriginDate.AddSeconds(timestamp);
                    AppendToDisplay(finalDate);
                }
            }
            else if ((matchCollection = DatetimeStringRegex.Matches(clipText))?.Count > 0)
            {
                foreach (Match matchValue in matchCollection)
                {
                    var finalDate = DateTime.Parse(matchValue.Value);
                    AppendToDisplay(finalDate);
                }
                
            }
            else
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
    }
}
