using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OsrmRoutesEditor.Controls
{
    /// <summary>
    /// Interaction logic for LogWindowControl.xaml
    /// </summary>
    public partial class LogWindowControl : UserControl
    {
        public LogWindowControl()
        {
            InitializeComponent();          
            Paragraph p = LogWindowRichTextBox.Document.Blocks.FirstBlock as Paragraph;
            if (p != null) p.LineHeight = 2;
        }

        public void AddMessage(string message)
        {
            LogWindowRichTextBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + " " + message + "\n");
            LogWindowRichTextBox.CaretPosition = LogWindowRichTextBox.Document.ContentEnd;
            LogWindowRichTextBox.ScrollToEnd();
        }

        public void Clear()
        {
            LogWindowRichTextBox.SelectAll();
            LogWindowRichTextBox.Selection.Text = String.Empty;
        }
    }
}
