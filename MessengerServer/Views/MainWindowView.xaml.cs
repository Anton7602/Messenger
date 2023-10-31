using MessengerServer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MessengerServer.Views
{
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
            Closing += (DataContext as MainWindowViewModel).OnWindowClosing;
            KeyDown += (DataContext as MainWindowViewModel).OnKeyPressed;
            (DataContext as MainWindowViewModel).MessagesList.CollectionChanged += MessagesUpdated;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void MessagesUpdated(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MessengesListbox.Items.Count>1)
            {
                MessengesListbox.ScrollIntoView(MessengesListbox.Items[MessengesListbox.Items.Count - 1]);
            }
        }

        private void MessengesListbox_Loaded(object sender, RoutedEventArgs e)
        {
            if (MessengesListbox.Items.Count > 1)
            {
                MessengesListbox.ScrollIntoView(MessengesListbox.Items[MessengesListbox.Items.Count - 1]);
            }
        }
    }
}
