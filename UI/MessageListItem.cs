using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Local_Messenger
{
    class MessageListItem : Button
    {
        public enum Themes
        {
            SenderTop, SenderMiddle, SenderBottom, SenderSolo,
            ReceiverTop, ReceiverMiddle, ReceiverBottom, ReceiverSolo
        }

        static private Style SenderTopTheme { get; }
        static private Style SenderMiddleTheme { get; }
        static private Style SenderBottomTheme { get; }
        static private Style SenderSoloTheme { get; }
        static private Style ReceiverTopTheme { get; }
        static private Style ReceiverMiddleTheme { get; }
        static private Style ReceiverBottomTheme { get; }
        static private Style ReceiverSoloTheme { get; }

        static private Style[] themes = new Style[8];


        static private Style buttonTheme { get; }
        static MessageListItem()
        {
            // create each of the styles
            for (int index = 0; index < 8; index++)
            {
                themes[index] = new Style(typeof(Button));
                ControlTemplate template = new ControlTemplate(typeof(Button));

                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Border));
                factory.SetValue(Border.BackgroundProperty, Brushes.Green);
                factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5, 10, 10, 5));
                factory.SetValue(HeightProperty, 30);

                template.VisualTree = factory;

                factory.AppendChild(new FrameworkElementFactory(typeof(TextBlock)));
                factory.FirstChild.SetValue(TextBlock.TextProperty, "Message");
                factory.FirstChild.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                factory.FirstChild.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                factory.FirstChild.SetValue(TextBlock.PaddingProperty, new Thickness(5));

                themes[index].Setters.Add(new Setter { Property = Button.TemplateProperty, Value = template });
            }
        }


        Message message;
        public MessageListItem(Message thisMessage)
        {
            message = thisMessage;
            Text = message.content;

        }
    }
}
