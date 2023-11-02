using DevExpress.Data;
using DevExpress.Xpf.Core;
using MessengerClient.Models;
using MessengerClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace MessengerClient.Views
{
    public partial class MessengerMainWindow : ThemedWindow
    {
        public static string ApplicationID
        {
            get { return string.Format("InteractiveNotifications_{0}", AssemblyInfo.VersionShort.Replace(".", "_")); }
        }
        public static string ApplicationName
        {
            get { return "Interactive Notifications"; }
        }

        public MessengerMainWindow()
        {
            InitializeComponent();
            RegisterApplication();
            //Connecting views event with viewmodels handlers
            Closing += (DataContext as MainWindowViewModel).OnWindowClosing;
            KeyDown += (DataContext as MainWindowViewModel).OnKeyPressed;
            //Subscribes to Messages collection updates to properly scroll MessagesHolder to the last position
            (DataContext as MainWindowViewModel).MessagesList.CollectionChanged += MessagesUpdated;
        }

        /// <summary>
        /// Username validation sender
        /// </summary>
        /// <param name="sender">EditText</param>
        /// <param name="e">EditText Content</param>
        private void Username_Validate(object sender, DevExpress.Xpf.Editors.ValidationEventArgs e)
        {
            if (e.Value == null || e.Value.Equals(String.Empty))
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = "Username shouldn't be empty";
                return;
            }

            if (!e.Value.ToString().All(c => char.IsLetterOrDigit(c)))
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = "Username should contain only letters and digits";
                return;
            }

            if (e.Value.ToString().Length>20)
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = "Username shouldn't be longer than 20 symbols";
                return;
            }
            return;
        }

        /// <summary>
        /// Listener for status changes that colors changeEditText border in the proper color
        /// </summary>
        /// <param name="sender">Status EditText</param>
        /// <param name="e">Current visible status</param>
        private void Status_Decorate(object sender, DevExpress.Xpf.Editors.ValidationEventArgs e)
        {
            switch (e.Value)
            {
                case "Connected":
                    StatusTextBox.BorderBrush = Brushes.DarkGreen;
                    break;
                case "Establishing Connection":
                    StatusTextBox.BorderBrush = Brushes.LightGreen;
                    break;
                case "Disconnected":
                    StatusTextBox.BorderBrush = Brushes.DarkGray;
                    break;
                default:
                    StatusTextBox.BorderBrush = Brushes.DarkRed;
                    break;
            }
        }


        /// <summary>
        /// Triggers on Messenges collection update and scrolls MessagesHolder to a last position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessagesUpdated(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MessageHolder.VisibleItems.Count>1)
            {
                MessageHolder.ScrollIntoView(MessageHolder.VisibleItems.Count-1);
            }
        }

        // DevExpress Notification code.
        #region DevExpress Notification
        public static void SendActivatorMessage(string arguments)
        {
            MessageBox.Show("Activator invoked! Notification id = " + arguments);
        }
        void RegisterApplication()
        {
            if (!ShellHelper.IsApplicationShortcutExist(ApplicationName))
            {
                ShellHelper.RegisterComServer(typeof(CustomNotificationActivator));
                ShellHelper.TryCreateShortcut(ApplicationID, ApplicationName, null, typeof(CustomNotificationActivator));
            }
        }
        #endregion
    }
}
