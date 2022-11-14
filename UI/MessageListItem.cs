using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

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

                FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
                border.SetValue(Border.HeightProperty, 30.0);

                template.VisualTree = border;

                FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
                border.AppendChild(text);
                text.SetBinding(TextBlock.TextProperty, new Binding { RelativeSource = RelativeSource.TemplatedParent, Path = new PropertyPath("Content") });
                text.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                text.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                text.SetValue(TextBlock.PaddingProperty, new Thickness(5));

                // if sender
                if (index <= (int) Themes.SenderSolo)
                {
                    border.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                    border.SetValue(Border.BackgroundProperty, Brushes.Violet);
                    text.SetValue(TextBlock.ForegroundProperty, Brushes.Black);

                }
                else // if receiver
                {
                    border.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    border.SetValue(Border.BackgroundProperty, Brushes.DarkSlateGray);
                    text.SetValue(TextBlock.ForegroundProperty, Brushes.White);
                }

                switch (index)
                {
                    case (int)Themes.SenderTop:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 10, 10, 5));
                        break;
                    case (int)Themes.SenderMiddle:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5, 10, 10, 5));
                        break;
                    case (int)Themes.SenderBottom:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5, 10, 10, 10));
                        break;
                    case (int)Themes.SenderSolo:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 10, 10, 10));
                        break;

                    case (int)Themes.ReceiverTop:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 10, 5, 10));
                        break;
                    case (int)Themes.ReceiverMiddle:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 5, 5, 10));
                        break;
                    case (int)Themes.ReceiverBottom:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 5, 10, 10));
                        break;
                    case (int)Themes.ReceiverSolo:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 10, 10, 10));
                        break;
                }

                themes[index].Setters.Add(new Setter { Property = Button.TemplateProperty, Value = template });
            }
        }

        private MessageListItem(Message thisMessage)
        {
            message = thisMessage;
            this.SetBinding(Button.ContentProperty, new Binding { Source = thisMessage, Path = new PropertyPath("content") });
        }

        Message message;
        public MessageListItem(Message thisMessage, Person user) : this(thisMessage)
        {
            if (user == thisMessage.sender)
            {
                this.Style = themes[(int)Themes.SenderSolo];
            }
            else
            {
                this.Style = themes[(int)Themes.ReceiverSolo];
            }
        }

        public MessageListItem(Message thisMessage, Themes style) : this(thisMessage)
        {
            this.Style = themes[(int)style];
        }
    }
}
