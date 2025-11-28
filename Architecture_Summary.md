# NodeNetwork 專案架構分析報告

經過對 `D:\workspace\NodeNetwork` 的程式碼庫分析，以下是該解決方案的架構摘要。這是一個基於 **WPF** 的節點編輯器框架，其核心設計高度依賴 **Reactive Programming (反應式編程)** 模式。

## 1. 核心架構風格 (Architectural Style)
*   **MVVM (Model-View-ViewModel)**: 嚴格遵循 MVVM 模式，UI 邏輯與業務邏輯分離。
*   **Reactive Programming**: 深度整合 **ReactiveUI** 與 **DynamicData**。所有的狀態變更（如節點的增刪、連接的建立、數值的傳遞）都透過 Observable Streams (觀察者串流) 進行管理與傳播，而非傳統的事件 (Events)。

## 2. 專案職責劃分

### A. `NodeNetwork` (核心庫)
這是框架的基礎層，負責處理「圖 (Graph)」的拓撲結構與基礎交互，但不強制規定數據如何流動。
*   **NetworkViewModel**: 整個節點圖的根物件 (Root)，包含 `Nodes` 和 `Connections` 的集合 (使用 `SourceList` 管理)。
*   **NodeViewModel**: 所有節點的基類。開發者需繼承此類別來建立自定義節點。
*   **NodeInputViewModel / NodeOutputViewModel**: 代表節點上的輸入與輸出端口 (Ports)。
*   **ConnectionViewModel**: 代表連線。
*   **NNViewRegistrar**: 負責將 ViewModel 與 View 進行綁定 (使用 Splat Locator)。

### B. `NodeNetworkToolkit` (工具庫)
這是建立在核心庫之上的擴充層，提供了「有型別 (Typed)」的數據流實作與常用的 UI 組件。
*   **ValueNode**: 提供了泛型的 `ValueNodeInputViewModel<T>` 與 `ValueNodeOutputViewModel<T>`。這使得節點之間可以傳遞具體的數據類型 (如 `int`, `string` 等)。
*   **UI Helpers**: 包含 `NodeList` (左側可拖曳的節點列表)、`BreadcrumbBar` (導航列)、`ContextMenu` 等實用控制項。
*   **大多數應用程式應同時引用 Core 和 Toolkit。**

## 3. 數據流機制 (Data Flow Mechanism)
數據流是 **Push-based (推播式)** 的，完全基於 Rx (Reactive Extensions)。

*   **輸入 (Input)**: `ValueNodeInputViewModel<T>` 具有一個 `Value` 屬性 (通常是 `IObservable<T>` 或支援 `WhenAnyValue` 監聽)。
*   **輸出 (Output)**: `ValueNodeOutputViewModel<T>` 接受一個 `IObservable<T>` 作為其值源。
*   **處理邏輯**: 在自定義節點中，你使用 Rx 操作符 (如 `Select`, `CombineLatest`) 將輸入的變更轉換為輸出。

**範例邏輯 (參考 `SumNodeViewModel.cs`):**
```csharp
// 當 Input1 或 Input2 的值改變時，自動計算總和並推送到 Output
var sum = this.WhenAnyValue(vm => vm.Input1.Value, vm => vm.Input2.Value)
    .Select(_ => Input1.Value + Input2.Value);

Output = new ValueNodeOutputViewModel<int?> { Value = sum };
```

## 4. 擴充方式 (Extensibility)
開發者若要建立新的節點應用，通常需要：
1.  建立一個繼承自 `NodeViewModel` 的類別。
2.  在建構函式中實例化 `ValueNodeInputViewModel<T>` 和 `ValueNodeOutputViewModel<T>` 並加入 `Inputs`/`Outputs` 集合。
3.  定義輸入到輸出的 Rx 轉換邏輯。
4.  (可選) 自定義 View，或使用預設的 `NodeView`。

## 5. 依賴關係 (Dependencies)
*   **ReactiveUI**: 用於 MVVM 綁定與反應式邏輯。
*   **DynamicData**: 用於高效管理集合 (如節點列表、連線列表) 的變更通知。
*   **Splat**: 用於依賴注入 (Service Location) 與 View 註冊。

## 6. 資料管理擴充 (Data Management Extension)

為了整合舊有系統的產品資料與參數設定，`ActuatorApp` 專案導入了新的資料管理層。此舉嚴格遵循了 Core/UI 分離的原則，確保了業務邏輯與資料存取邏輯的解耦。

### 6.1 資源移植
*   **產品圖片**: 從 `DG_Programmer_Git/PGEProgrammer/Products` 資料夾將圖片資源移植到 `ActuatorApp/Assets/Products`。
*   **資料庫**: 將包含產品參數定義的 SQLite 資料庫檔案 `Database.db` 從 `DG_Programmer_Git/PGEProgrammer/Category/PGE1/Database.db` 複製到 `ActuatorApp/Assets/Database.db`。
*   **部署配置**: 修改 `ActuatorApp.csproj`，確保上述 `Assets` 資料夾及其內容會在應用程式建置時自動複製到輸出目錄，確保執行時資料的可存取性。

### 6.2 新增資料基礎設施層
為提供專責的資料存取能力，新增了一個專案：
*   **`ActuatorApp.Infrastructure`**: 一個 .NET Standard 2.0 的類別庫，用於封裝所有與外部資料來源（如 `Database.db`）互動的邏輯。
    *   **依賴**: 引用 `Microsoft.Data.Sqlite` 作為 SQLite 驅動，並使用 `Dapper` 作為輕量級 ORM 框架以簡化資料映射操作。

### 6.3 實體 (Entities) 定義
在 **`ActuatorApp.Core/Entities`** 資料夾中，根據舊有系統的資料表結構 (`PGE開發文檔.md` 中定義的 `Touch`, `Tcsync`, `TCS`, `Controlbox`, `Control`, `Columns`, `Actuator`, `Actlevel`)，定義了對應的 C# 實體類別 (POCOs)。這些實體類別是純粹的資料容器，不包含任何業務邏輯或資料庫存取細節，可被 Core 層和 UI 層共享。

### 6.4 Repository 模式實作
*   **介面定義**: 在 **`ActuatorApp.Core/Interfaces/IProductRepository.cs`** 中定義了資料存取介面，例如 `GetTcsyncsAsync()`, `GetControlsAsync()` 等，規範了資料操作的契約。
*   **具體實作**: 在 **`ActuatorApp.Infrastructure/Repositories/ProductRepository.cs`** 中，實作了 `IProductRepository` 介面。此實作負責建立 SQLite 資料庫連線，並利用 Dapper 執行 SQL 查詢，將查詢結果映射到 Core 層定義的實體對象。

### 6.5 數據流影響
新的資料管理層使得 `ActuatorApp` 能夠讀取舊系統中定義的詳細產品參數和圖像路徑。這些資料未來可以被整合到 `ProductCatalog` 或各個 `NodeViewModel` 中，用於豐富節點的顯示資訊、配置選項或提供更精細的連線驗證規則。此改動增強了系統對產品資料的處理能力，為後續的參數調整功能開發奠定了基礎。