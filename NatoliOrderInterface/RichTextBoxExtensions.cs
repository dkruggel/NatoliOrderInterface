using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NatoliOrderInterface
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, SolidColorBrush brush)
        {
            TextRange textRange = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            textRange.Text = text;
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }
        public static void ReplaceText(this RichTextBox box, string text, SolidColorBrush brush)
        {
            TextRange textRange = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
            textRange.Text = text;
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }
        public static string GetText(this RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                rtb.Document.ContentStart,
                rtb.Document.ContentEnd
            );
            return textRange.Text;
        }
    }
}
