using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Local_Messenger
{
    public static class ColorScheme
    {
        public static SolidColorBrush BackgroundColor = (SolidColorBrush)Application.Current.Resources["BackgroundColor"];
        public static SolidColorBrush ForegroundColor = (SolidColorBrush)Application.Current.Resources["ForegroundColor"];
        public static SolidColorBrush ForegroundDarkenedColor = (SolidColorBrush)Application.Current.Resources["ForegroundDarkenedColor"];


        public static SolidColorBrush ButtonBackground = (SolidColorBrush)Application.Current.Resources["ButtonBackground"];
        public static SolidColorBrush ButtonForeground = (SolidColorBrush)Application.Current.Resources["ButtonForeground"];

        public static SolidColorBrush TextBoxBackground = (SolidColorBrush)Application.Current.Resources["TextBoxBackground"];
        public static SolidColorBrush TextBoxForeground = (SolidColorBrush)Application.Current.Resources["TextBoxForeground"];

        public static SolidColorBrush SentBackground = (SolidColorBrush)Application.Current.Resources["SentBackground"];
        public static SolidColorBrush SentForeground = (SolidColorBrush)Application.Current.Resources["SentForeground"];

        public static SolidColorBrush ReceivedBackground = (SolidColorBrush)Application.Current.Resources["ReceivedBackground"];
        public static SolidColorBrush ReceivedForeground = (SolidColorBrush)Application.Current.Resources["ReceivedForeground"];
    }
}
