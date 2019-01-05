using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TelloAltitudeUnlocker
{
    public partial class App : Application
    {
        private readonly bool _consoleLogErrors = true;
        private readonly bool _consoleLogInfo = true;
        private const string appleKey = "";
        private const string androidKey = "";

        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += LogError;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; ;

            var vm = new MainPageViewModel(LogError, Log);
            vm.Init();
            MainPage = new MainPage { BindingContext = vm };
        }

        protected override void OnStart()
        {
            AppCenter.Start($"ios={appleKey};android={androidKey}", typeof(Analytics), typeof(Crashes));
            Analytics.TrackEvent("AppStart");
        }

        protected override void OnSleep()
        {
            Analytics.TrackEvent("AppSleep");
        }

        protected override void OnResume()
        {
            Analytics.TrackEvent("AppResume");
        }

        public void Log(string item, IDictionary<string,string> props = null)
        {
            if (_consoleLogInfo)
                System.Diagnostics.Debug.WriteLine(item);

            Analytics.TrackEvent(item, props);
        }

        public void LogError(Exception e, string info = null)
        {
            if (_consoleLogErrors)
                System.Diagnostics.Debug.WriteLine(e.Message);

            Crashes.TrackError(e);
        }

        private void LogError(object _, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;
            LogError(ex);
        }

        private void TaskScheduler_UnobservedTaskException(object _, UnobservedTaskExceptionEventArgs e)
        {
            LogError(e.Exception);
        }
    }
}
