using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Controls.Primitives;




using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Local_Messenger
{
    class ChatListItem : RadioButton
    {
        static public MainWindow window;
        static private Style buttonTheme { get; }

        private Person person;

        static ChatListItem()
        {
            // creates the toggle button style
            buttonTheme = new Style(typeof(RadioButton), new Style(typeof(ToggleButton)));
            ControlTemplate buttonTemplate = new ControlTemplate(typeof(RadioButton));
            FrameworkElementFactory elementFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            buttonTemplate.VisualTree = elementFactory;
            buttonTheme.Setters.Add(new Setter { Property = Button.TemplateProperty, Value = buttonTemplate });
        }

        public ChatListItem(Person thisPerson)
        {
            this.person = thisPerson;
            // initialize each element
            Image facePic = new Image();
            Border imageBorder = new Border(); // image border
            TextBlock nameBox = new TextBlock();
            TextBlock timeBox = new TextBlock();
            Grid grid = new Grid();

            // set up facePic
            facePic.Source = thisPerson.contactPhoto;
            facePic.Width = 50;
            facePic.Height = 50;
            facePic.Stretch = Stretch.UniformToFill;

            ImageBrush imageBrush = new ImageBrush(facePic.Source);
            imageBrush.Stretch = Stretch.UniformToFill;

            imageBorder.Height = 50;
            imageBorder.Width = 50;
            imageBorder.Clip = new RectangleGeometry(new Rect(0, 0, imageBorder.Width, imageBorder.Height), imageBorder.Width / 2, imageBorder.Height / 2);
            imageBorder.Child = facePic;
            imageBorder.Background = Brushes.Red;

            // set up nameBox
            nameBox.Text = thisPerson.name;
            nameBox.FontSize = 20;
            nameBox.Foreground = Brushes.White;
            nameBox.HorizontalAlignment = HorizontalAlignment.Left;
            nameBox.VerticalAlignment = VerticalAlignment.Center;

            // set up last message
            TextBlock lastMessage = new TextBlock();
            lastMessage.FontSize = 12;
            lastMessage.Foreground = Brushes.Gray;
            lastMessage.HorizontalAlignment = HorizontalAlignment.Left;
            lastMessage.VerticalAlignment = VerticalAlignment.Center;
            if (thisPerson.messages.Count != 0)
            {
                Message last = thisPerson.messages.Last();
                string preview = string.Format("{0}: {1}", last.sender.name, last.content);
                if (preview.Length >= 25)
                {
                    preview = preview.Substring(0, 25) + "...";
                }
                lastMessage.Text = preview;
            }

            Grid nameGrid = new Grid();
            for (int rows = 0; rows < 2; rows++)
            {
                nameGrid.RowDefinitions.Add(new RowDefinition());
            }

            nameGrid.Children.Add(nameBox);
            nameGrid.Children.Add(lastMessage);

            Grid.SetRow(nameBox, 0);
            Grid.SetRow(lastMessage, 1);


            // set up timeBox
            if (thisPerson.messages.Count != 0)
            {
                DateTime timeStamp = thisPerson.messages.Last().sentTimeStamp;
                TimeSpan difference = DateTime.Now - timeStamp;
                if (difference.TotalHours < 1) // show in minutes
                {
                    timeBox.Text = string.Format("{0} min", (int) difference.TotalMinutes);
                }
                else if (difference.TotalDays < 1) // show time from today
                {
                    timeBox.Text = timeStamp.ToString("h:mm tt");
                }
                else if (difference.TotalDays < 7) // show the day in words
                {
                    timeBox.Text = timeStamp.ToString("ddd");
                }
                else if (difference.TotalDays < 90) // show the month and day
                {
                    timeBox.Text = timeStamp.ToString("MMM d");
                }
                else // show the date
                {
                    timeBox.Text = timeStamp.ToString("d");
                }
            }
            else
            {
                timeBox.Text = "";
            }
            timeBox.FontSize = 12;
            timeBox.Foreground = Brushes.Gray;
            timeBox.HorizontalAlignment = HorizontalAlignment.Right;
            timeBox.VerticalAlignment = VerticalAlignment.Top;

            // set up grid
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();
            colDef1.Width = new GridLength(1.2, GridUnitType.Star);
            colDef2.Width = new GridLength(3, GridUnitType.Star);
            colDef3.Width = new GridLength(1.2, GridUnitType.Star);

            grid.ColumnDefinitions.Add(colDef1);
            grid.ColumnDefinitions.Add(colDef2);
            grid.ColumnDefinitions.Add(colDef3);


            // add items to grid
            grid.Children.Add(imageBorder);
            grid.Children.Add(nameGrid);
            grid.Children.Add(timeBox);

            Grid.SetColumn(imageBorder, 0);
            Grid.SetColumn(nameGrid, 1);
            Grid.SetColumn(timeBox, 2);

            //// for debugging
            //grid.ShowGridLines = true;

            //// set up background
            //imageBorder.HorizontalAlignment = HorizontalAlignment.Center;
            //imageBorder.VerticalAlignment = VerticalAlignment.Center;
            //imageBorder.Height = 250;
            //imageBorder.Width = 250;
            //imageBorder.Cursor = Cursors.Hand;
            //imageBorder.Clip = new RectangleGeometry(new Rect(0, 0, 250, 250), 60, 60);
            //imageBorder.Child = grid;


            // set up this button
            this.Foreground = Brushes.White;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.Content = grid;
            this.Cursor = Cursors.Hand;
            grid.Background = Brushes.Transparent; // needed so the cursor shows across the whole grid
            //this.Content = imageBorder;
            this.Click += new RoutedEventHandler(OnChatClicked);

            // styles the button as a toggle button
            this.Margin = new Thickness(5);
            this.Style = buttonTheme;
        }

        private void OnChatClicked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            TextBlock Message_Name = window.Message_Name;
            ListView Message_List = window.Messages_List;
            ChatListItem thisItem = sender as ChatListItem;
            // change the messages shown

            Message_Name.Text = person.name;
            Message_List.Items.Clear();

            MessageListItem.AddToListView(Message_List, person.messages.ToArray(), window.me);
            
        }
        //public BitmapImage Convert(System.Drawing.Image img)
        //{
        //    using (var memory = new System.IO.MemoryStream())
        //    {
        //        img.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        //        memory.Position = 0;
        //        var bitmapImage = new BitmapImage();
        //        bitmapImage.BeginInit();
        //        bitmapImage.StreamSource = memory;
        //        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmapImage.EndInit();
        //        return bitmapImage;
        //    }
        //}


        //private System.Drawing.Bitmap WayOne(string file, System.Windows.Controls.Image showimage)
        //{
        //    using (System.Drawing.Image i = new System.Drawing.Bitmap(file))
        //    {
        //        System.Drawing.Point p1 = new System.Drawing.Point();
        //        int wth = (int)showimage.Width;
        //        int hig = (int)showimage.Height;
        //        p1.X = (i.Width / 2) - (wth / 2);
        //        p1.Y = (i.Height / 2) - (hig / 2);
        //        System.Drawing.Bitmap newimage = new System.Drawing.Bitmap(wth, hig);
        //        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newimage);
        //        g.DrawImage(i, new System.Drawing.Rectangle(0, 0, wth, hig), new System.Drawing.Rectangle(p1, new System.Drawing.Size(wth, hig)), System.Drawing.GraphicsUnit.Pixel);
        //        return newimage;
        //    }
        //}
    }
}
