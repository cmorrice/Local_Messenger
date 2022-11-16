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

        static private Style[] themes = new Style[Enum.GetNames(typeof(Themes)).Length];

        static MessageListItem()
        {
            // create each of the styles
            for (int index = 0; index < themes.Length; index++)
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
                    border.SetValue(Border.BackgroundProperty, ColorScheme.SentBackground);
                    text.SetValue(TextBlock.ForegroundProperty, ColorScheme.SentForeground);

                }
                else // if receiver
                {
                    border.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    border.SetValue(Border.BackgroundProperty, ColorScheme.ReceivedBackground);
                    text.SetValue(TextBlock.ForegroundProperty, ColorScheme.ReceivedForeground);
                }

                switch (index)
                {
                    case (int)Themes.SenderTop:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 10, 5, 10));
                        break;
                    case (int)Themes.SenderMiddle:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 5, 5, 10));
                        break;
                    case (int)Themes.SenderBottom:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 5, 10, 10));
                        break;
                    case (int)Themes.SenderSolo:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 10, 10, 10));
                        break;

                    case (int)Themes.ReceiverTop:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(10, 10, 10, 5));
                        break;
                    case (int)Themes.ReceiverMiddle:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5, 10, 10, 5));
                        break;
                    case (int)Themes.ReceiverBottom:
                        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5, 10, 10, 10));
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

        public static void AddToListView(ListView list, Message[] messages, Person sender)
        {
            int length = messages.Length;
            // if there is only message or this message is solo
            if (length == 1 || messages[0].sender != messages[1].sender)
            {
                if (messages[0].sender == sender)
                {
                    list.Items.Add(new MessageListItem(messages[0], Themes.SenderSolo));
                }
                else
                {
                    list.Items.Add(new MessageListItem(messages[0], Themes.ReceiverSolo));
                }
            }
            else if (messages[0].sender == sender)
            {
                list.Items.Add(new MessageListItem(messages[0], Themes.SenderTop));
            }
            else
            {
                list.Items.Add(new MessageListItem(messages[0], Themes.ReceiverTop));
            }

            for (int index = 1; index < length - 1; index++)
            {

                Message last = messages[index - 1];
                Message message = messages[index];
                Message next = messages[index + 1];

                if (message.sender == sender) // if we sent message
                {
                    if (last.sender == sender) // if middle or bottom
                    {
                        if (next.sender == sender) // if middle
                        {
                            list.Items.Add(new MessageListItem(message, Themes.SenderMiddle));
                        }
                        else // if bottom
                        {
                            list.Items.Add(new MessageListItem(message, Themes.SenderBottom));
                        }
                    }
                    else // if top or solo
                    {
                        if (next.sender == sender) // if top
                        {
                            list.Items.Add(new MessageListItem(message, Themes.SenderTop));
                        }
                        else // if solo
                        {
                            list.Items.Add(new MessageListItem(message, Themes.SenderSolo));
                        }
                    }
                }
                else // if we received message
                {
                    if (last.sender != sender) // if middle or bottom
                    {
                        if (next.sender != sender) // if middle
                        {
                            list.Items.Add(new MessageListItem(message, Themes.ReceiverMiddle));
                        }
                        else // if bottom
                        {
                            list.Items.Add(new MessageListItem(message, Themes.ReceiverBottom));
                        }
                    }
                    else // if top or solo
                    {
                        if (next.sender != sender) // if top
                        {
                            list.Items.Add(new MessageListItem(message, Themes.ReceiverTop));
                        }
                        else // if solo
                        {
                            list.Items.Add(new MessageListItem(message, Themes.ReceiverSolo));
                        }
                    }
                }


            }

            if (messages.Length > 1)
            {
                Message last = messages[messages.Length - 2];
                Message message = messages[messages.Length - 1];

                if (message.sender == sender) // if sent
                {
                    if (message.sender == last.sender)
                    {
                        list.Items.Add(new MessageListItem(message, Themes.SenderBottom));
                    }
                    else
                    {
                        list.Items.Add(new MessageListItem(message, Themes.SenderSolo));
                    }
                }
                else // if received
                {
                    if (message.sender != last.sender)
                    {
                        list.Items.Add(new MessageListItem(message, Themes.ReceiverBottom));
                    }
                    else
                    {
                        list.Items.Add(new MessageListItem(message, Themes.ReceiverSolo));
                    }
                }
                
            }
        }
    }
}
