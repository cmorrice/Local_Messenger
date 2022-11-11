using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Local_Messenger
{
    class MessageListItem : TextBlock
    {
        static private Style buttonTheme { get; }
        static MessageListItem()
        {
            // creates the toggle button style
            buttonTheme = new Style(typeof(RadioButton), new Style(typeof(ToggleButton)));
            ControlTemplate buttonTemplate = new ControlTemplate(typeof(RadioButton));
            FrameworkElementFactory elementFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            buttonTemplate.VisualTree = elementFactory;
            buttonTheme.Setters.Add(new Setter { Property = Button.TemplateProperty, Value = buttonTemplate });
        }


        Message message;
        public MessageListItem(Message thisMessage)
        {
            message = thisMessage;
            Text = message.content;

        }
    }
}
