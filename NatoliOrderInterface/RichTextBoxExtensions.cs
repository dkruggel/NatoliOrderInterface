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
        public static string GetString(this RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }
    }
}
