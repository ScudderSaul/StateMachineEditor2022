using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StateMachineEditor2022.Code;

namespace StateMachineEditor2022.Controls
{
    /// <summary>
    /// Interaction logic for CheckItem.xaml
    /// </summary>
    public partial class CheckItem : UserControl
    {
        public CheckItem()
        {
            InitializeComponent();
            SelectColor = Colors.Blue;
            UnSelectColor = Colors.LightBlue;
            NoEdit = false;
        }

        public Color SelectColor { get; set; }
        public Color UnSelectColor { get; set; }

        public bool NoEdit { get; set; }

        public void SetSelected()
        {
           // CheckControl.IsChecked = true;
          //  NameImage.Visibility = Visibility.Visible;
            if (NoEdit == false)
            {
                OriginalText.Visibility = Visibility.Collapsed;
                NameText.Visibility = Visibility.Visible;
                NameImage.Visibility = Visibility.Visible;
            }
            else
            {
                OriginalText.Visibility = Visibility.Visible;
                NameText.Visibility = Visibility.Collapsed;
                NameImage.Visibility = Visibility.Visible;
            }
        }

        public void SetUnSelected()
        {
           // CheckControl.IsChecked = false;
         // NameImage.Visibility = Visibility.Collapsed;
            if (NoEdit == false)
            {
                OriginalText.Visibility = Visibility.Visible;
                NameText.Visibility = Visibility.Collapsed;
                NameImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                OriginalText.Visibility = Visibility.Visible; 
                NameText.Visibility = Visibility.Collapsed;
                NameImage.Visibility = Visibility.Collapsed;
            }
        }

       

        public Action<CheckItem> SelectionAction { get; set;  }

        private void NameText_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectionAction != null)
            {
                SelectionAction(this);
                RaiseSelectedCheckItemEvent();
            }
        }

        public delegate void SelectedCheckItemHandler(object sender, SelectedCheckItemEventArgs e);

        // Declare the event. 
        public event SelectedCheckItemHandler SelectedCheckItemEvent;

        protected virtual void RaiseSelectedCheckItemEvent()
        {
            SelectedCheckItemEventArgs args = new SelectedCheckItemEventArgs();
            args.Item = this;
            args.OriginalText = this.OriginalText.Text;

            if (SelectedCheckItemEvent != null)
                SelectedCheckItemEvent(this, args);
        }


    }
}
