using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.Notifications.Management;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Background;
using Windows.Media.Capture;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Devices;

namespace Notify
{

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

        private async void GetList_Click(object sender, RoutedEventArgs e)
        {
            IReadOnlyList<UserNotification> notifs = await listener.GetNotificationsAsync(NotificationKinds.Toast);
            NotificationsList.ItemsSource = notifs;
            foreach(var item in notifs)
            {
                Debug.WriteLine(item.Notification.Visual.Bindings[0].GetTextElements());                
            }
            Debug.WriteLine("Список получен");
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            listener.ClearNotifications();
            Debug.WriteLine("Уведомления очищены");
        }

        private void SendToast_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Background_Click(object sender, RoutedEventArgs e)
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
            if(!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals("UserNotificationChanged")))
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

        private async void Torch_Click(object sender, RoutedEventArgs e)
        {
            CaptureElement captureUI = new CaptureElement();

            try
            {
                MediaCapture mediaCapture = new MediaCapture();

                var cam = await GetCam();

                await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = cam.Id,
                    AudioDeviceId = string.Empty,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    PhotoCaptureSource = PhotoCaptureSource.VideoPreview
                });
                captureUI.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();

                torchControl = mediaCapture.VideoDeviceController.TorchControl;

                if (torchControl.Supported)
                {
                    Debug.WriteLine("Вспышка поддерживается");
                    

                }
                else Debug.WriteLine("Вспышка не поддерживается");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            

            

        }

        public async Task<DeviceInformation> GetCam()
        {
            return await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);
        }

        private async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }
    }
}
