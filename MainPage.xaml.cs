using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using Windows.Media.Devices;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Notify
{
    public sealed partial class MainPage : Page
    {
        UserNotificationListener listener;
        public static TorchControl torchControl;
        public static Frame frame;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            frame = this.Frame;
        }

        public void Initialize()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                Debug.WriteLine("API уведомлений поддерживаются");
                StatusUpdate("Выполняется настройка");
                SetListener();
                Ring.IsActive = true;
            }
            else StatusUpdate("API не поддерживается");
        }

        public async void SetListener()
        {
            listener = UserNotificationListener.Current;

            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            switch (accessStatus)
            {
                case UserNotificationListenerAccessStatus.Allowed:
                    GetAccessBackgroundTask();
                    Debug.WriteLine("Уведомления доступны");
                    
                    break;
                case UserNotificationListenerAccessStatus.Denied:
                    StatusUpdate("Уведомления недоступны. Откройте");
                    break;
                case UserNotificationListenerAccessStatus.Unspecified:
                    Debug.WriteLine("Неизвестно");
                    break;
                default:
                    Status.Text = "Уведомления недоступны";
                    break;
            }
        }

        public async void GetAccessBackgroundTask()
        {
            BackgroundAccessStatus backgroundStatus = await BackgroundExecutionManager.RequestAccessAsync();
            switch (backgroundStatus)
            {
                case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                    Debug.WriteLine("Фоновые задачи доступны частично");
                    BackgroundRegister();
                    break;
                case BackgroundAccessStatus.AlwaysAllowed:
                    Debug.WriteLine("Фоновые задачи полностью поддерживаются");
                    BackgroundRegister();
                    break;
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                    Debug.WriteLine("Фоновые задачи отключены или недоступны");
                    break;
                case BackgroundAccessStatus.DeniedByUser:
                    Status.Text = "Фоновые задачи недоступны";
                    break;
                default:
                    Status.Text = "Фоновые задачи недоступны";
                    break;
            }
        }

        private void BackgroundRegister()
        {
            if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals("UserNotificationChanged")))
            {
                var builder = new BackgroundTaskBuilder()
                {
                    Name = "UserNotificationChanged"
                };

                builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
                Debug.WriteLine("Триггер установлен");
                builder.Register();
                Debug.WriteLine("Задача зарегистрирована");
                IconChange.Begin();
            }


        }


        private void OnSettings(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(View.Settings));
        }

        private void OnAbout(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(View.About));
        }

        private void SetTask(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void StatusUpdate(string message)
        {
            TextShow.Begin();
            Status.Text = message;

            
          
        }
    }
}
