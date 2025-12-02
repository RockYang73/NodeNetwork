using System; // Added
using System.IO;
using System.Windows;
using ActuatorApp.Infrastructure.Repositories;
using ActuatorApp.Services;
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

            // 取得應用程式基礎目錄
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 實例化 ProductRepository
            string dbPath = Path.Combine(baseDir, "Assets", "Database.db");
            var productRepository = new ProductRepository($"Data Source={dbPath}");

            // 實例化 AutoConnectService
            var autoConnectService = new AutoConnectService();

            // 實例化 ProductImageService
            string jsonConfigPath = Path.Combine(baseDir, "Assets", "products.json");
            string productsBasePath = Path.Combine(baseDir, "Assets", "Products");
            var productImageService = new ProductImageService(jsonConfigPath, productsBasePath);

            // 將 MainViewModel 設定為 DataContext 和 ViewModel
            var viewModel = new MainViewModel(productRepository, autoConnectService, productImageService);
            var mainWindow = new MainWindow
            {
                ViewModel = viewModel
            };
            mainWindow.Show();
        }
    }
}
