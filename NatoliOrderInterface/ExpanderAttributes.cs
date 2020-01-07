using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NatoliOrderInterface
{
    class ExpanderAttributes
    {
        public SolidColorBrush Background { get; set; }
        public string BackgroundColor { get; set; }
        public SolidColorBrush Foreground { get; set; }
        public string ForegroundColor { get; set; }
        public FontWeight TextFontWeight { get; set; }
        public string TextFontWeightString { get; set; }
        public FontStyle TextFontStyle { get; set; }
        public string TextFontStyleString { get; set; }

        public ExpanderAttributes()
        {
            Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFFFF");
            Foreground = new SolidColorBrush(Colors.Black);
            TextFontWeight = FontWeights.Normal;
            TextFontStyle = FontStyles.Normal;
        }

        public ExpanderAttributes(SolidColorBrush background)
        {
            Background = background;
            BackgroundColor = background.Color.ToString();
            Foreground = new SolidColorBrush(Colors.Black);
            TextFontWeight = FontWeights.Normal;
            TextFontStyle = FontStyles.Normal;
        }

        public ExpanderAttributes(SolidColorBrush background, SolidColorBrush foreground)
        {
            Background = background;
            BackgroundColor = background.Color.ToString();
            Foreground = foreground;
            ForegroundColor = foreground.Color.ToString();
            TextFontWeight = FontWeights.Normal;
            TextFontStyle = FontStyles.Normal;
        }

        public ExpanderAttributes(SolidColorBrush background, SolidColorBrush foreground, FontWeight fontWeight)
        {
            Background = background;
            BackgroundColor = background.Color.ToString();
            Foreground = foreground;
            ForegroundColor = foreground.Color.ToString();
            TextFontWeight = fontWeight;
            TextFontWeightString = fontWeight.ToString();
            TextFontStyle = FontStyles.Normal;
        }

        public ExpanderAttributes(SolidColorBrush background, SolidColorBrush foreground, FontWeight fontWeight, FontStyle fontStyle)
        {
            Background = background;
            BackgroundColor = background.Color.ToString();
            Foreground = foreground;
            ForegroundColor = foreground.Color.ToString();
            TextFontWeight = fontWeight;
            TextFontWeightString = fontWeight.ToString();
            TextFontStyle = fontStyle;
            TextFontStyleString = fontStyle.ToString();
        }
    }
}
