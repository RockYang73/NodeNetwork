using System; // Added
using System.IO;
using System.Windows;
using ActuatorApp.Infrastructure.Repositories;
using ActuatorApp.ViewModels;
using ActuatorApp.Views; // Added
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

            // 實例化 ProductRepository
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Database.db");
            var productRepository = new ProductRepository($"Data Source={dbPath}");

            // 實例化 AutoConnectService
            var autoConnectService = new ActuatorApp.Services.AutoConnectService();

            // 將 MainViewModel 設定為 DataContext 和 ViewModel
            var viewModel = new MainViewModel(productRepository, autoConnectService);
            var mainWindow = new MainWindow
            {
                ViewModel = viewModel
            };
            mainWindow.Show();
        }
    }
}
