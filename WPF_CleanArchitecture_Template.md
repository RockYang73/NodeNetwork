# WPF Clean Architecture 基礎專案架構規範

## 文件資訊
| 項目 | 內容 |
|------|------|
| **版本** | 1.0 |
| **建立日期** | 2025年12月2日 |
| **適用範圍** | WPF 桌面應用程式專案 |
| **架構模式** | Clean Architecture + MVVM |

---

## 1. 架構概述

### 1.1 分層架構圖

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Presentation Layer (UI)                          │
│                        [ProjectName]                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌────────────┐  │
│  │   Views     │  │ ViewModels  │  │  Converters │  │  Behaviors │  │
│  │   (XAML)    │  │   (MVVM)    │  │             │  │            │  │
│  └─────────────┘  └─────────────┘  └─────────────┘  └────────────┘  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                  │
│  │  Adapters   │  │  Services   │  │ Extensions  │                  │
│  │(外部框架)    │  │ (UI 服務)   │  │(DI 註冊等)  │                  │
│  └─────────────┘  └─────────────┘  └─────────────┘                  │
└─────────────────────────────────────────────────────────────────────┘
                                │ depends on
┌─────────────────────────────────────────────────────────────────────┐
│                     Application Layer (Use Cases)                    │
│                    [ProjectName].Application                         │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                      Use Cases / Commands                    │    │
│  │              (業務流程、應用程式邏輯)                          │    │
│  └─────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                         DTOs                                 │    │
│  │                  (資料傳輸物件)                               │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
                                │ depends on
┌─────────────────────────────────────────────────────────────────────┐
│                        Core Layer (Domain)                           │
│                       [ProjectName].Core                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │   Entities   │  │  Interfaces  │  │    Enums     │               │
│  │  (領域模型)   │  │  (抽象定義)  │  │   (列舉)     │               │
│  └──────────────┘  └──────────────┘  └──────────────┘               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │   Services   │  │  Validation  │  │  Exceptions  │               │
│  │ (領域服務)    │  │  (驗證邏輯)  │  │ (自訂例外)   │               │
│  └──────────────┘  └──────────────┘  └──────────────┘               │
└─────────────────────────────────────────────────────────────────────┘
                                │ implemented by
┌─────────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                              │
│                  [ProjectName].Infrastructure                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │ Repositories │  │   External   │  │    Logging   │               │
│  │ (資料存取)    │  │   Services   │  │   (日誌)     │               │
│  └──────────────┘  └──────────────┘  └──────────────┘               │
│  ┌──────────────┐  ┌──────────────┐                                 │
│  │  Persistence │  │    Config    │                                 │
│  │   (ORM)      │  │   (設定)     │                                 │
│  └──────────────┘  └──────────────┘                                 │
└─────────────────────────────────────────────────────────────────────┘
```

### 1.2 依賴方向

```
UI (Presentation) → Application → Core ← Infrastructure
```

**重要規則**:
- Core 層**不依賴**任何外層
- Infrastructure 層**實作** Core 層定義的介面
- UI 層可**直接依賴** Core 層 (跳過 Application 層)

---

## 2. 專案結構範本

### 2.1 Solution 結構

```
📁 [SolutionName]/
├── 📄 [SolutionName].sln
├── 📄 README.md
├── 📄 .gitignore
├── 📄 Directory.Build.props          # 共用建置設定
│
├── 📁 src/
│   ├── 📁 [ProjectName]/             # UI Layer (Presentation)
│   ├── 📁 [ProjectName].Application/ # Application Layer (可選)
│   ├── 📁 [ProjectName].Core/        # Core Layer (Domain)
│   └── 📁 [ProjectName].Infrastructure/ # Infrastructure Layer
│
├── 📁 tests/
│   ├── 📁 [ProjectName].Core.Tests/
│   ├── 📁 [ProjectName].Application.Tests/
│   └── 📁 [ProjectName].IntegrationTests/
│
└── 📁 docs/
    ├── 📄 Architecture.md
    └── 📄 API.md
```

### 2.2 Core 專案結構

```
📁 [ProjectName].Core/
├── 📄 [ProjectName].Core.csproj      # netstandard2.0 或 net6.0+
│
├── 📁 Entities/                       # 領域實體
│   ├── 📄 BaseEntity.cs              # 基類 (Id, CreatedAt 等)
│   └── 📄 [EntityName].cs
│
├── 📁 Interfaces/                     # 抽象介面
│   ├── 📁 Repositories/              # 資料存取介面
│   │   └── 📄 I[Entity]Repository.cs
│   ├── 📁 Services/                  # 服務介面
│   │   └── 📄 I[Service]Service.cs
│   └── 📄 IUnitOfWork.cs             # 工作單元 (可選)
│
├── 📁 Enums/                          # 列舉定義
│   └── 📄 [EnumName].cs
│
├── 📁 ValueObjects/                   # 值物件 (可選)
│   └── 📄 [ValueObject].cs
│
├── 📁 Services/                       # 領域服務 (純邏輯)
│   └── 📄 [DomainService].cs
│
├── 📁 Validation/                     # 驗證規則
│   └── 📄 [ValidationRule].cs
│
├── 📁 Exceptions/                     # 自訂例外
│   ├── 📄 DomainException.cs
│   └── 📄 [SpecificException].cs
│
└── 📁 Extensions/                     # 擴充方法
    └── 📄 [Type]Extensions.cs
```

### 2.3 Infrastructure 專案結構

```
📁 [ProjectName].Infrastructure/
├── 📄 [ProjectName].Infrastructure.csproj
│
├── 📁 Repositories/                   # 資料存取實作
│   ├── 📄 BaseRepository.cs
│   └── 📄 [Entity]Repository.cs
│
├── 📁 Persistence/                    # 資料庫相關
│   ├── 📄 AppDbContext.cs            # EF Core DbContext
│   └── 📁 Configurations/            # Entity 設定
│       └── 📄 [Entity]Configuration.cs
│
├── 📁 Services/                       # 外部服務實作
│   └── 📄 [ExternalService].cs
│
├── 📁 Logging/                        # 日誌實作
│   └── 📄 FileLogger.cs
│
└── 📄 DependencyInjection.cs          # DI 註冊擴充
```

### 2.4 UI 專案結構 (WPF + MVVM)

```
📁 [ProjectName]/
├── 📄 [ProjectName].csproj
├── 📄 App.xaml
├── 📄 App.xaml.cs
│
├── 📁 Views/                          # 視圖 (XAML)
│   ├── 📄 MainWindow.xaml
│   ├── 📄 MainWindow.xaml.cs
│   ├── 📁 Pages/                     # 頁面
│   │   └── 📄 [Page]View.xaml
│   ├── 📁 Controls/                  # 自訂控制項
│   │   └── 📄 [Control]View.xaml
│   ├── 📁 Dialogs/                   # 對話框
│   │   └── 📄 [Dialog]View.xaml
│   ├── 📁 Editors/                   # 編輯器
│   │   └── 📄 [Editor]View.xaml
│   └── 📁 Converters/                # 值轉換器
│       └── 📄 [Type]Converter.cs
│
├── 📁 ViewModels/                     # ViewModel
│   ├── 📄 ViewModelBase.cs           # ViewModel 基類
│   ├── 📄 MainViewModel.cs
│   ├── 📁 Pages/
│   │   └── 📄 [Page]ViewModel.cs
│   └── 📁 Dialogs/
│       └── 📄 [Dialog]ViewModel.cs
│
├── 📁 Models/                         # UI 專用模型 (非領域)
│   └── 📄 [UIModel].cs
│
├── 📁 Services/                       # UI 層服務
│   ├── 📄 INavigationService.cs
│   ├── 📄 NavigationService.cs
│   ├── 📄 IDialogService.cs
│   └── 📄 DialogService.cs
│
├── 📁 Adapters/                       # 外部框架適配器
│   └── 📄 [Framework]Adapter.cs
│
├── 📁 Behaviors/                      # XAML 行為
│   └── 📄 [Behavior]Behavior.cs
│
├── 📁 Extensions/                     # DI 註冊等擴充
│   └── 📄 ServiceCollectionExtensions.cs
│
├── 📁 Resources/                      # 資源檔
│   ├── 📁 Styles/                    # 樣式
│   │   └── 📄 [Control]Styles.xaml
│   ├── 📁 Images/                    # 圖片
│   └── 📁 Themes/                    # 主題
│
├── 📁 Assets/                         # 靜態資源
│   └── 📄 (fonts, db, etc.)
│
└── 📁 Properties/
    └── 📄 AssemblyInfo.cs
```

---

## 3. 命名規範

### 3.1 專案命名

| 層級 | 命名格式 | 範例 |
|------|----------|------|
| UI | `[ProductName]` | `ActuatorApp` |
| Application | `[ProductName].Application` | `ActuatorApp.Application` |
| Core | `[ProductName].Core` | `ActuatorApp.Core` |
| Infrastructure | `[ProductName].Infrastructure` | `ActuatorApp.Infrastructure` |
| Tests | `[ProjectName].Tests` | `ActuatorApp.Core.Tests` |

### 3.2 檔案命名

| 類型 | 命名規則 | 範例 |
|------|----------|------|
| Entity | `[Name].cs` | `Product.cs` |
| Interface | `I[Name].cs` | `IProductRepository.cs` |
| Repository | `[Entity]Repository.cs` | `ProductRepository.cs` |
| Service | `[Name]Service.cs` | `ValidationService.cs` |
| ViewModel | `[Name]ViewModel.cs` | `MainViewModel.cs` |
| View | `[Name]View.xaml` | `MainView.xaml` |
| Converter | `[Type]Converter.cs` | `BoolToVisibilityConverter.cs` |
| Enum | `[Name].cs` (複數形式可選) | `NodeType.cs` |

### 3.3 命名空間

```csharp
// Core
namespace [ProductName].Core.Entities
namespace [ProductName].Core.Interfaces
namespace [ProductName].Core.Interfaces.Repositories
namespace [ProductName].Core.Interfaces.Services
namespace [ProductName].Core.Enums
namespace [ProductName].Core.Validation

// Infrastructure
namespace [ProductName].Infrastructure.Repositories
namespace [ProductName].Infrastructure.Persistence
namespace [ProductName].Infrastructure.Services

// UI
namespace [ProductName].Views
namespace [ProductName].ViewModels
namespace [ProductName].Services
namespace [ProductName].Adapters
namespace [ProductName].Extensions
```

---

## 4. 依賴注入設定

### 4.1 Infrastructure 層 DI 註冊

```csharp
// [ProjectName].Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;

namespace [ProjectName].Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            string connectionString)
        {
            // 資料庫
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            // Repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
```

### 4.2 UI 層 DI 註冊

```csharp
// [ProjectName]/Extensions/ServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;

namespace [ProjectName].Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPresentation(
            this IServiceCollection services)
        {
            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();

            return services;
        }
    }
}
```

### 4.3 App.xaml.cs 設定

```csharp
// [ProjectName]/App.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace [ProjectName]
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // 註冊各層服務
                    services.AddInfrastructure(GetConnectionString());
                    services.AddPresentation();
                    
                    // 註冊主視窗
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            
            base.OnExit(e);
        }

        private string GetConnectionString()
        {
            var dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                "Assets", 
                "Database.db");
            return $"Data Source={dbPath}";
        }
    }
}
```

---

## 5. ViewModel 基類範本

### 5.1 使用 ReactiveUI

```csharp
using ReactiveUI;
using System.Reactive;

namespace [ProjectName].ViewModels
{
    public abstract class ViewModelBase : ReactiveObject
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
    }
}
```

### 5.2 使用 CommunityToolkit.Mvvm

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace [ProjectName].ViewModels
{
    public abstract partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;
    }
}
```

---

## 6. 常用套件建議

### 6.1 Core 層 (最小依賴)

```xml
<!-- 理想情況：無外部依賴 -->
<!-- 如需 JSON 處理 -->
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

### 6.2 Infrastructure 層

```xml
<!-- ORM -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<!-- 或使用 Dapper -->
<PackageReference Include="Dapper" Version="2.1.0" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />

<!-- 日誌 -->
<PackageReference Include="Serilog" Version="3.1.0" />
```

### 6.3 UI 層

```xml
<!-- MVVM 框架 (擇一) -->
<PackageReference Include="ReactiveUI.WPF" Version="19.0.0" />
<!-- 或 -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.0" />

<!-- DI -->
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />

<!-- UI 控制項 -->
<PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
```

---

## 7. 檢查清單

### 7.1 新專案建立檢查

- [ ] 建立 Solution 結構 (src/, tests/, docs/)
- [ ] 建立 Core 專案 (netstandard2.0 或 net6.0+)
- [ ] 建立 Infrastructure 專案
- [ ] 建立 UI 專案 (WPF)
- [ ] 設定專案參考 (依賴方向正確)
- [ ] 設定 DI Container
- [ ] 建立 ViewModelBase
- [ ] 建立基礎資料夾結構

### 7.2 程式碼品質檢查

- [ ] Core 層無 UI 框架依賴
- [ ] 所有 Repository 介面定義在 Core
- [ ] ViewModel 不直接存取資料庫
- [ ] 使用介面而非具體類別
- [ ] 遵循命名規範

### 7.3 架構檢查

- [ ] 依賴方向正確 (外 → 內)
- [ ] Core 層可獨立編譯
- [ ] Infrastructure 可替換
- [ ] UI 框架可替換 (理論上)

---

## 8. 快速建立腳本

### 8.1 PowerShell 腳本

```powershell
# create-wpf-solution.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$SolutionName,
    [string]$OutputPath = "."
)

$basePath = Join-Path $OutputPath $SolutionName

# 建立資料夾結構
New-Item -ItemType Directory -Force -Path "$basePath/src"
New-Item -ItemType Directory -Force -Path "$basePath/tests"
New-Item -ItemType Directory -Force -Path "$basePath/docs"

# 建立 Solution
dotnet new sln -n $SolutionName -o $basePath

# 建立 Core 專案
dotnet new classlib -n "$SolutionName.Core" -o "$basePath/src/$SolutionName.Core" -f netstandard2.0
dotnet sln "$basePath/$SolutionName.sln" add "$basePath/src/$SolutionName.Core"

# 建立 Infrastructure 專案
dotnet new classlib -n "$SolutionName.Infrastructure" -o "$basePath/src/$SolutionName.Infrastructure"
dotnet sln "$basePath/$SolutionName.sln" add "$basePath/src/$SolutionName.Infrastructure"
dotnet add "$basePath/src/$SolutionName.Infrastructure" reference "$basePath/src/$SolutionName.Core"

# 建立 WPF 專案
dotnet new wpf -n $SolutionName -o "$basePath/src/$SolutionName"
dotnet sln "$basePath/$SolutionName.sln" add "$basePath/src/$SolutionName"
dotnet add "$basePath/src/$SolutionName" reference "$basePath/src/$SolutionName.Core"
dotnet add "$basePath/src/$SolutionName" reference "$basePath/src/$SolutionName.Infrastructure"

# 建立 Core 子資料夾
$coreFolders = @("Entities", "Interfaces", "Interfaces/Repositories", "Interfaces/Services", "Enums", "Validation", "Exceptions")
foreach ($folder in $coreFolders) {
    New-Item -ItemType Directory -Force -Path "$basePath/src/$SolutionName.Core/$folder"
}

# 建立 Infrastructure 子資料夾
$infraFolders = @("Repositories", "Persistence", "Services")
foreach ($folder in $infraFolders) {
    New-Item -ItemType Directory -Force -Path "$basePath/src/$SolutionName.Infrastructure/$folder"
}

# 建立 UI 子資料夾
$uiFolders = @("Views", "Views/Pages", "Views/Dialogs", "Views/Converters", "ViewModels", "Models", "Services", "Adapters", "Extensions", "Resources", "Assets")
foreach ($folder in $uiFolders) {
    New-Item -ItemType Directory -Force -Path "$basePath/src/$SolutionName/$folder"
}

Write-Host "Solution '$SolutionName' created successfully at $basePath" -ForegroundColor Green
```

### 8.2 使用方式

```powershell
.\create-wpf-solution.ps1 -SolutionName "MyApp" -OutputPath "D:\Projects"
```

---

## 9. 參考資料

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft Docs - .NET Application Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/)
- [ReactiveUI Documentation](https://www.reactiveui.net/)
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
