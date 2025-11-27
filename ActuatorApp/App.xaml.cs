using System.Windows;
using NodeNetwork;

namespace ActuatorApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 註冊 NodeNetwork 預設視圖
            NNViewRegistrar.RegisterSplat();
        }
    }
}
