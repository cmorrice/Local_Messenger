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

using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Windows.Media.Imaging;

namespace Local_Messenger
{
    class MessageListItem : Button
    {
        public enum Themes
        {
            SenderTop, SenderMiddle, SenderBottom, SenderSolo,
            ReceiverTop, ReceiverMiddle, ReceiverBottom, ReceiverSolo,

            SenderImage,
            ReceiverImage,
            
            SenderFile,
            ReceiverFile
        }

        static private Style[] themes = new Style[Enum.GetNames(typeof(Themes)).Length];

        static private Style ContextMenuTheme = new Style(typeof(ContextMenu));
        static private Style MenuItemTheme = new Style(typeof(MenuItem));

        private Message message;

        static MessageListItem()
        {
            // create each of the normal message styles
            for (int index = 0; index <= (int) Themes.ReceiverSolo; index++)
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

            // create the image styles
            for (int index = (int) Themes.SenderImage; index <= (int) Themes.ReceiverImage; index++)
            {
                themes[index] = new Style(typeof(Button));
                ControlTemplate template = new ControlTemplate(typeof(Button));

                FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
                border.SetValue(Border.HeightProperty, 80.0);
                border.SetValue(Border.WidthProperty, 80.0);
                border.SetValue(Border.MarginProperty, new Thickness(5));

                template.VisualTree = border;


                FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
                
                image.SetBinding(Image.SourceProperty, new Binding { RelativeSource = RelativeSource.TemplatedParent, Path = new PropertyPath("Content") });
                image.SetValue(Image.WidthProperty, 80.0);
                image.SetValue(Image.HeightProperty, 80.0);
                image.SetValue(Image.StretchProperty, Stretch.UniformToFill);

                //FrameworkElementFactory imageBrush = new FrameworkElementFactory(typeof(ImageBrush));
                //imageBrush.SetValue(ImageBrush.ImageSourceProperty, image);
                //imageBrush.SetValue(ImageBrush.StretchProperty, Stretch.UniformToFill);

                border.SetValue(Border.ClipProperty, new RectangleGeometry(new Rect(0, 0, 80.0, 80.0), 80.0 / 2, 80.0 / 2));

                border.AppendChild(image);

                // if sender
                if (index == (int)Themes.SenderImage)
                {
                    border.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                    border.SetValue(Border.BackgroundProperty, ColorScheme.SentBackground);

                }
                else if (index == (int) Themes.ReceiverImage)
                {
                    border.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    border.SetValue(Border.BackgroundProperty, ColorScheme.ReceivedBackground);
                }

                themes[index].Setters.Add(new Setter { Property = Button.TemplateProperty, Value = template });
            }



            // create the ContextMenu style
            ControlTemplate contextTemplate = new ControlTemplate(typeof(ContextMenu));
            FrameworkElementFactory contextBorder = new FrameworkElementFactory(typeof(Border));
            contextTemplate.VisualTree = contextBorder;

            FrameworkElementFactory contextScroll = new FrameworkElementFactory(typeof(ScrollViewer));
            contextBorder.AppendChild(contextScroll);
            contextScroll.SetValue(ScrollViewer.CanContentScrollProperty, true);
            contextScroll.SetResourceReference(ScrollViewer.StyleProperty, new ComponentResourceKey(typeof(FrameworkElement), "MenuScrollViewer"));
            contextScroll.AppendChild(new FrameworkElementFactory(typeof(ItemsPresenter)));
            ContextMenuTheme.Setters.Add(new Setter { Property = MenuItem.TemplateProperty, Value = contextTemplate });

            // create the MenuItem styles
            ControlTemplate menuTemplate = new ControlTemplate(typeof(MenuItem));
            FrameworkElementFactory menuBorder = new FrameworkElementFactory(typeof(Border));
            menuBorder.SetValue(Border.BackgroundProperty, ColorScheme.ButtonBackground);
            menuBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            menuTemplate.VisualTree = menuBorder;

            FrameworkElementFactory menuText = new FrameworkElementFactory(typeof(TextBlock));
            menuBorder.AppendChild(menuText);
            menuText.SetBinding(TextBlock.TextProperty, new Binding { RelativeSource = RelativeSource.TemplatedParent, Path = new PropertyPath("Tag") });
            menuText.SetValue(TextBlock.FontSizeProperty, 14.0);
            menuText.SetValue(TextBlock.ForegroundProperty, ColorScheme.ButtonForeground);
            menuText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            menuText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            menuText.SetValue(TextBlock.MarginProperty, new Thickness(5));
            MenuItemTheme.Setters.Add(new Setter { Property = MenuItem.TemplateProperty, Value = menuTemplate });
            //Trigger styleTrigger = new Trigger { Property = MenuItem.IsMouseOverProperty, Value = true };
            //styleTrigger.Setters.Add(new Setter { Property = MenuItem.BackgroundProperty, Value = new SolidColorBrush(Color.FromRgb(62, 113, 124))});
            //contextTemplate.Triggers.Add(styleTrigger);
        }

        private MessageListItem(Message thisMessage)
        {
            // set up the message itself
            this.message = thisMessage;
            this.SetBinding(Button.ContentProperty, new Binding { Source = thisMessage, Path = new PropertyPath("content") });

            // set up the context menu
            ContextMenu menu = new ContextMenu();
            menu.Style = ContextMenuTheme;

            // if image or file, give the option to save
            if (thisMessage.type == Message.MessageType.image || thisMessage.type == Message.MessageType.file)
            {
                MenuItem save = new MenuItem();
                save.Tag = "Save";
                save.Style = MenuItemTheme;
                save.Click += new RoutedEventHandler(SaveItemClicked);
                menu.Items.Add(save);
            }

            MenuItem delete = new MenuItem();
            delete.Tag = "Delete";
            delete.Style = MenuItemTheme;
            delete.Click += new RoutedEventHandler(DeleteItemClicked);

            menu.Items.Add(delete);

            this.ContextMenu = menu;
        }

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

        private void SaveItemClicked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            BitmapImage image = this.message.content as BitmapImage;
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            FileStream file = new FileStream(string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, "Image.jpg"), System.IO.FileMode.Create);
            encoder.Save(file);
        }

        private void DeleteItemClicked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            MainWindow window = Application.Current.MainWindow as MainWindow;
            Person me = window.me;

            // get the other person
            Person other = (this.message.sender == me) ? this.message.receiver : this.message.sender;
            int index = other.messages.IndexOf(this.message);
            if (index == -1) // if message wasn't found for some reason???
            {
                return;
            }

            other.messages.Remove(this.message);
            // refresh page
            window.refreshWindow();
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

                // REALLY JANKY JUST FOR TESTING
                if (message.type == Message.MessageType.image)
                {
                    if (message.sender == sender)
                    {
                        list.Items.Add(new MessageListItem(message, Themes.SenderImage));
                    }
                    else
                    {
                        list.Items.Add(new MessageListItem(message, Themes.ReceiverImage));
                    }
                    return;
                }
                // END OF REALLY JANKY

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
                    if (message.sender == last.sender)
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
