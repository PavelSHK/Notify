using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Media.Devices;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Notify
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        UserNotificationListener listener;
        public static TorchControl torchControl;

        public MainPage()
        {
            this.InitializeComponent();

            Initialize();
        }

        public void Initialize()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                Debug.WriteLine("API уведомлений поддерживаются");
                SetListener();
           }
            else Debug.WriteLine("API уведомлений не поддежриваются");
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
                    Debug.WriteLine("Уведомления недоступны");
                    break;
                case UserNotificationListenerAccessStatus.Unspecified:
                    Debug.WriteLine("Неизвестно");
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
                    Debug.WriteLine("Фоновые задачи отключены пользователем");
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
            }


        }


        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(View.Settings));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
