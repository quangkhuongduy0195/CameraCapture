using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CameraCapture.Renderers.Camera;
using Prism.Common;
using Prism.AppModel;

namespace CameraCapture
{
    public partial class App : Application
    {
        CameraView cameraView;
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnResume()
        {
            InvokeBaseLifeCycleOnResume();
            base.OnResume();
        }

        void InvokeBaseLifeCycleOnResume()
        {
            if (MainPage == null)
                return;
            Page page;
            page = PageUtilities.GetCurrentPage(MainPage);
            PageUtilities.InvokeViewAndViewModelAction<IApplicationLifecycleAware>(page, x => x.OnResume());
        }
    }
}
