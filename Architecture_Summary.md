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
