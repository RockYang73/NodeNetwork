# ActuatorApp 規格文件

## 文件資訊
| 項目 | 內容 |
|------|------|
| **專案名稱** | ActuatorApp (電動推桿控制系統節點編輯器) |
| **基於框架** | NodeNetwork (WPF + ReactiveUI) |
| **建立日期** | 2025年11月27日 |
| **版本** | 3.0 (整合 Infrastructure 層與進階功能) |

---

## 1. 系統概述

### 1.1 目的
建立一個基於 NodeNetwork 框架的電動推桿控制系統視覺化配置工具，讓使用者能夠透過拖拉節點的方式配置電動推桿系統的連接關係。

### 1.2 系統架構
```
電源供應 (TP) → 電池 (TBB) → 控制盒 (TC) → 推桿 (TA)
                                    ↑
                              手控器 (TH/TFH/THP)
                                    ↓
                              配件 (TYC)
```

### 1.3 核心架構風格

| 架構模式 | 說明 |
|----------|------|
| **MVVM** | 嚴格遵循 Model-View-ViewModel 模式，UI 邏輯與業務邏輯分離 |
| **Reactive Programming** | 深度整合 ReactiveUI 與 DynamicData，所有狀態變更透過 Observable Streams 管理 |
| **Clean Architecture** | 三層分離架構 (Core → Infrastructure → UI)，依賴方向由外向內 |

### 1.4 軟體架構 (解耦設計)

```
┌─────────────────────────────────────────────────────────────────┐
│                        ActuatorApp (UI Layer)                    │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │   MainViewModel │  │ NodeViewModels  │  │     Views       │  │
│  │  (uses Catalog) │  │ (impl IActuator │  │   (XAML/WPF)    │  │
│  └────────┬────────┘  │     Node)       │  └─────────────────┘  │
│           │           └────────┬────────┘                        │
│  ┌────────▼────────────────────▼────────────────────────────┐   │
│  │              NodeNetworkNodeFactory (Adapter)             │   │
│  │           - Implements INodeFactory                       │   │
│  │           - Maps ProductDefinition → NodeViewModel        │   │
│  └──────────────────────────────┬───────────────────────────┘   │
└─────────────────────────────────┼───────────────────────────────┘
                                  │ depends on
┌─────────────────────────────────▼───────────────────────────────┐
│                   ActuatorApp.Core (Business Logic)              │
│                       (netstandard2.0)                           │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                       Interfaces                          │   │
│  │  - IActuatorNode (NodeType, ModelNumber, DisplayName)     │   │
│  │  - IActuatorPort (PortType, Name, ParentNode)             │   │
│  │  - IConnectionValidator (Validate)                        │   │
│  │  - INodeFactory (CreateNode, CreateNodeByModel)           │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                        Products                           │   │
│  │  - ProductDefinition (完整產品規格)                        │   │
│  │  - PortDefinition (端口規格)                               │   │
│  │  - CategoryDefinition (分類結構)                           │   │
│  │  - ProductCatalog (產品目錄服務)                           │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                       Validation                          │   │
│  │  - ConnectionRuleEngine (純商業邏輯驗證)                   │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                         Enums                             │   │
│  │  - NodeType, PortType, ProtocolType                       │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

**架構優勢:**
- **ActuatorApp.Core** 完全不依賴 NodeNetwork，可隨時遷移至其他節點編輯器套件
- 產品定義集中管理，可透過 JSON 檔案動態載入
- 連線驗證邏輯與 UI 框架分離

### 1.4 資料流類型
| 類型 | 說明 | 視覺呈現 | 流向 |
|------|------|----------|------|
| **Power** | 電流傳遞 | 藍色圓形端口 | TP → TBB → TC |
| **BinaryData** | 二進制控制信號 | 灰色方形端口 | TC ↔ TH, TC → TA |
| **Parameter** | 參數值 | 黃色菱形端口 | Parameter → TA |

---

## 2. 產品分類 (NodeGroup)

> **注意**: 本節描述系統標準分類結構。實際的產品型號清單、詳細分類歸屬（如 TBB 系列歸屬於 Battery）以及產品圖片路徑，目前由 **`Assets/products.json`** 進行動態定義與擴充。

### 2.1 Tab 結構
| Tab 名稱 | 節點系列 | 說明 |
|----------|----------|------|
| Control Box | TC 系列 | 控制盒，系統中樞 |
| Actuator | TA 系列 | 電動推桿 |
| Controls | TH / TFH / THP 系列 | 手控器（嵌入式/有線/無線/手持） |
| Accessories | TYC / TFA 系列 | 配件 |
| Power Supply | TP 系列 | 電源供應器 |
| Battery | TBB / BAT 系列 | 電池 |

### 2.2 Controls 子分類
| 子分類 | 包含型號 |
|--------|----------|
| Embedded | TFH2, TFH6, TFH7, TFH9, TFH15 |
| Wire Handset | TH1, TH4, TH7, TH7R, TH11, TH17, TFH35 |
| Wireless Handset | TH3, TH8, TH13, TH25, TFH22, TFH25, TFH27, TFH28, TFH34 |

---

## 3. 節點規格

### 3.1 TP 節點 (Power Supply)
| 項目 | 規格 |
|------|------|
| NodeType | PowerSupply |
| Input | (無) |
| Output | `Out` [Power] - 電流輸出 |
| 屬性 | 型號、電壓規格 |
| 背景色 | #4CAF50 (綠色) |

### 3.2 TBB 節點 (Battery)
| 項目 | 規格 |
|------|------|
| NodeType | Battery |
| Input | `In` [Power] - 來自 TP |
| Output | `Out` [Power] - 至 TC |
| 屬性 | 型號、容量 |
| 背景色 | #8BC34A (淺綠) |

### 3.3 TC 節點 (Control Box)
| 項目 | 規格 |
|------|------|
| NodeType | ControlBox |
| Input | `Power` [Power] - 電流輸入 |
| Input | `H1` [BinaryData] - 手控器信號 |
| Output | `M1~Mn` [BinaryData] - 推桿控制 (動態數量) |
| 屬性 | 型號、M 端口數量 (由後臺配置) |
| 背景色 | #2196F3 (藍色) |

### 3.4 TA 節點 (Actuator)
| 項目 | 規格 |
|------|------|
| NodeType | Actuator |
| Input | `TC` [BinaryData] - 控制信號 |
| Input | `Code` [Parameter] - 推桿編碼 |
| Input | `Stroke` [Parameter] - 行程設定 |
| Output | `a` [保留] - 附加元件 |
| 屬性 | 型號 |
| 背景色 | #FF9800 (橙色) |

### 3.5 TH/TFH 節點 (Control)
| 項目 | 規格 |
|------|------|
| NodeType | Control |
| Input | `a` [保留] - 附加元件 |
| Output | `TC` [BinaryData] - 控制信號 |
| 屬性 | 型號、`ProtocolType` (GPIO/Scan/TBUS 單選) |
| UI 元件 | Get ID 按鈕、通訊協定 Checkbox |
| 背景色 | #9C27B0 (紫色) |

### 3.6 TYC 節點 (Accessory - 延長線/分接線)
| 項目 | 規格 |
|------|------|
| NodeType | Accessory |
| 用途 | Control 與 Control Box 之間的橋樑 |
| Input | `IN1` [BinaryData] - 信號輸入 1 (來自 Control 或 TYC) |
| Input | `IN2` [BinaryData] - 信號輸入 2 (來自 Control 或 TYC) |
| Output | `Out` [BinaryData] - 信號輸出 (連接到 TC.H1 或 TYC) |
| 屬性 | 型號 |
| 背景色 | #607D8B (灰色) |

### 3.7 THP 節點 (Control - 手持控制器)
| 項目 | 規格 |
|------|------|
| NodeType | Control |
| Input | `TC` [BinaryData] - 控制信號 |
| 屬性 | 型號 |
| 背景色 | #9C27B0 (紫色) |

### 3.8 Parameter 節點 (黃色橢圓)
| 項目 | 規格 |
|------|------|
| NodeType | Parameter |
| Input | (無) |
| Output | `Value` [Parameter] - 參數值 |
| 編輯器 | 內建編輯器 + 支援外部連線 |
| 外觀 | 黃色橢圓形 |
| 背景色 | #FFC107 (黃色) |

---

## 4. 連線驗證規則

### 4.1 基本規則
- 端口類型必須匹配 (Power ↔ Power, BinaryData ↔ BinaryData, Parameter ↔ Parameter)
- 不允許同一節點的輸出連接到自身的輸入

### 4.2 Power 流向限制
```
允許: TP.Out → TBB.In
允許: TBB.Out → TC.Power
禁止: 其他 Power 連接
```

### 4.3 BinaryData 流向限制
```
允許: TC.M1~Mn → TA.TC           (推桿控制)
允許: TH/TFH/THP.TC → TC.H1     (Control 直連 ControlBox)
允許: TH/TFH/THP.TC → TYC.IN    (Control 連接延長線/分接線)
允許: TYC.Out → TC.H1            (配件連接控制盒)
允許: TYC.Out → TYC.IN           (配件級聯)
```

**使用範例:**
```
TH ─────────────────→ TC.H1          ✓ 直連
TH ──→ TYC ──→ TC.H1                 ✓ 單手控器經延長線
TH ──┐
     ├──→ TYC ──→ TC.H1              ✓ 雙手控器經分接線匯流
TH ──┘
TH ──→ TYC ──→ TYC ──→ TC.H1         ✓ 級聯
```

### 4.4 Parameter 流向限制
```
允許: Parameter.Value → TA.Code
允許: Parameter.Value → TA.Stroke
允許: Parameter.Value → 其他需要參數的端口
```

---

## 5. 列舉定義

### 5.1 PortType
```csharp
public enum PortType
{
    Power,      // 電流
    BinaryData, // 二進制控制信號
    Parameter   // 參數值
}
```

### 5.2 NodeType
```csharp
public enum NodeType
{
    PowerSupply,  // 電源供應器 (TP)
    Battery,      // 電池 (TBB)
    ControlBox,   // 控制盒 (TC)
    Actuator,     // 推桿 (TA)
    Control,      // 手控器 (TH/TFH/THP)
    Accessory,    // 配件 (TYC)
    Parameter     // 參數節點
}
```

### 5.3 ProtocolType
```csharp
public enum ProtocolType
{
    GPIO,
    Scan,
    TBUS
}
```

---

## 6. NodeNetwork 核心框架機制

### 6.1 核心 ViewModel 職責

| 類別 | 職責 |
|------|------|
| **NetworkViewModel** | 管理整個節點網絡，包含所有節點與連線的集合 |
| **NodeViewModel** | 表示單一節點，包含輸入/輸出端點集合 |
| **NodeInputViewModel** / **NodeOutputViewModel** | 表示節點的輸入/輸出端點 |
| **ConnectionViewModel** | 表示兩端點間的連線 |
| **PendingConnectionViewModel** | 使用者正在拖曳建立的暫存連線 |

### 6.2 Reactive 數據流機制

NodeNetwork 採用 **Push-based 響應式架構**，所有狀態變更自動傳播：

```csharp
// 範例：當 Input 連線變更時，自動計算新值
this.WhenAnyValue(vm => vm.Input.Values)      // 觀察連線集合
    .SelectMany(values => values              // 攤平每個連線的值
        .Select(v => v.Value)                 // 取得實際資料
        .CombineLatest())                     // 合併最新值
    .Subscribe(data => ProcessData(data));   // 處理資料
```

### 6.3 連線驗證流程

```
用戶拖曳連線
     │
     ▼
┌─────────────────────────────────────┐
│ PendingConnectionViewModel.Validation │
│   (IObservable<ValidationResult>)    │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│   每當目標端點變化時觸發驗證         │
│   - 檢查 PortType 是否相符           │
│   - 呼叫 ConnectionRuleEngine        │
│   - 回傳 ValidationResult            │
└─────────────────┬───────────────────┘
                  │
          ┌───────┴───────┐
          ▼               ▼
     ┌────────┐      ┌────────┐
     │ IsValid │      │ Invalid │
     │  =true  │      │ =false  │
     └────┬───┘      └────┬───┘
          │               │
          ▼               ▼
     允許連線         阻止連線
                    (顯示錯誤訊息)
```

---

## 7. 進階功能模組

### 7.1 Infrastructure 層 (資料存取)

```
📁 ActuatorApp.Infrastructure/
├── ActuatorApp.Infrastructure.csproj
└── 📁 Repositories/
    └── ProductRepository.cs
```

**IProductRepository 介面** (定義於 Core 層):
```csharp
public interface IProductRepository
{
    Task<IEnumerable<ProductDefinition>> GetAllProductsAsync();
    Task<ProductDefinition> GetProductByModelAsync(string modelNumber);
    Task<IEnumerable<ProductDefinition>> GetProductsByCategoryAsync(NodeType category);
    
    // 舊系統資料表存取方法
    Task<IEnumerable<Tcsync>> GetTcsyncsAsync();
    Task<IEnumerable<Control>> GetControlsAsync();
    Task<IEnumerable<Controlbox>> GetControlboxesAsync();
    Task<IEnumerable<Actuator>> GetActuatorsAsync();
}
```

**ProductRepository 實作** (Infrastructure 層):
- 使用 **Dapper** 作為微型 ORM
- 連接 **SQLite** 本地資料庫
- 提供非同步資料存取方法

### 7.1.1 暫存產品資料來源 (products.json)

在系統開發初期與過渡階段，為了快速驗證產品清單與圖片顯示，系統採用混合式資料來源：
1. **硬編碼預設值**: `ProductCatalog.CreateDefault()` 提供基礎核心產品定義。
2. **JSON 配置檔**: `Assets/products.json` 作為擴充資料來源，用於定義大量的產品型號、分類歸屬與圖片路徑。
   - **格式**:
     ```json
     {
       "products": [
         { "modelNumber": "TBB2", "nodeType": "Battery", "category": "Battery", "imagePath": "Battery/TBB2.png" }
       ]
     }
     ```
   - **載入機制**: 應用程式啟動時，`MainViewModel` 呼叫 `ProductCatalog.MergeFromJson()` 動態合併 JSON 資料。

### 7.1.2 資料來源抽換策略

為確保系統能平滑遷移至正式資料庫 (SQLite) 或遠端 API，架構設計保留了彈性：

1. **現狀 (Phase 1)**:
   - 資料流: `products.json` → `ProductCatalog`
   - 優點：快速迭代，無需維護資料庫工具，適合原型開發。

2. **目標 (Phase 2 - SQLite/Database)**:
   - 資料流: `SQLite DB` → `IProductRepository` → `ProductCatalog`
   - **抽換步驟**:
     1. 完善 `ActuatorApp.Infrastructure` 中的 `ProductRepository` 實作。
     2. 將 `products.json` 的內容遷移至 SQLite `Products` 資料表。
     3. 修改 `MainViewModel` 初始化邏輯，移除 `MergeFromJson`，改為呼叫 `repository.GetAllProductsAsync()` 並填入 `ProductCatalog`。

3. **未來 (Phase 3 - Web API)**:
   - 資料流: `Cloud API` → `IProductRepository (HttpImpl)` → `ProductCatalog`
   - **抽換步驟**:
     1. 實作 `ProductHttpRepository` (繼承 `IProductRepository`)。
     2. 在 `App.xaml.cs` 的依賴注入容器中，將 `IProductRepository` 的註冊由 `SqliteProductRepository` 替換為 `ProductHttpRepository`。

### 7.2 資源移植與部署

#### 7.2.1 產品圖片
```
來源: DG_Programmer_Git/PGEProgrammer/Products/
目標: ActuatorApp/Assets/Products/
```

#### 7.2.2 SQLite 資料庫
```
來源: DG_Programmer_Git/PGEProgrammer/Category/PGE1/Database.db
目標: ActuatorApp/Assets/Database.db
```

#### 7.2.3 部署配置 (csproj)
```xml
<ItemGroup>
  <None Update="Assets\Database.db">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="Assets\Products\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### 7.3 資料庫實體 (Entities)

定義於 `ActuatorApp.Core/Entities/ProductEntities.cs`:

| 實體類別 | 對應資料表 | 用途 |
|----------|------------|------|
| Touch | Touch | 觸控螢幕資料 |
| Tcsync | Tcsync | 同步控制盒資料 |
| TCS | TCS | TCS 系列資料 |
| Controlbox | Controlboxes | 控制盒資料 |
| Control | Controls | 手控器資料 |
| Columns | Columns | 升降柱資料 |
| Actuator | Actuators | 推桿資料 |
| Actlevel | Actlevel | 推桿等級資料 |
| PowerSupply | PowerSupplies | 電源供應器資料 |
| Battery | Batteries | 電池資料 |
| Accessory | Accessories | 配件資料 |

### 7.4 Repository 模式實作

```csharp
// ActuatorApp.Infrastructure/Repositories/ProductRepository.cs
public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;
    
    public ProductRepository(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }
    
    public async Task<IEnumerable<Control>> GetControlsAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<Control>("SELECT * FROM Controls");
    }
    
    public async Task<IEnumerable<Controlbox>> GetControlboxesAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<Controlbox>("SELECT * FROM Controlboxes");
    }
    
    // ... 其他方法
}
```

### 7.5 數據流整合

新的資料管理層使得 `ActuatorApp` 能夠：
- 讀取舊系統中定義的詳細產品參數
- 載入產品圖像路徑
- 整合到 `ProductCatalog` 或各個 `NodeViewModel` 中
- 提供更精細的連線驗證規則

### 7.6 自動連線服務 (AutoConnectService)

定義於 `ActuatorApp/Services/`:

```csharp
public interface IAutoConnectService
{
    void TryAutoConnect(NodeViewModel newNode, NetworkViewModel network);
}
```

**連接邏輯流程:**
1. **掃描輸入**: 遍歷新節點的所有輸入端口 (Input Ports)
2. **尋找匹配**: 對於每個未連接的輸入端口，遍歷網路上現有所有節點的輸出端口
3. **相容性驗證**:
   - 確保輸入與輸出的 `PortType` 相同
   - 呼叫 `ConnectionRules.ValidateConnection` 進行業務邏輯檢查
4. **建立連線**: 找到第一個符合條件的輸出端口後，自動建立連線

**整合方式:**
- **依賴注入**: `AutoConnectService` 在 `App.xaml.cs` 中被實例化，並注入到 `MainViewModel`
- **觸發時機**: 在 `MainViewModel.AddNode` 方法中，節點添加後立即呼叫

### 7.7 群組節點機制 (Group Node)

定義於 `ActuatorApp/ViewModels/Nodes/` 目錄:

**用途:** 將多個節點打包成單一群組節點，簡化複雜配置的視覺呈現。

#### 7.7.1 核心元件

| 元件 | 檔案位置 | 說明 |
|------|----------|------|
| `GroupNodeViewModel` | `Nodes/GroupNodeViewModel.cs` | 標準群組節點 ViewModel (使用預設 NodeView) |
| `ContainerGroupNodeViewModel` | `Nodes/ContainerGroupNodeViewModel.cs` | **容器樣式**群組節點 ViewModel |
| `GroupNodeView` | `Views/GroupNodeView.xaml` | 容器樣式群組節點的自定義視圖 |
| `NodeGrouper` | `NodeNetworkToolkit/Group/` | 執行分組邏輯的服務類別 |
| `ActuatorGroupIOBinding` | `ActuatorGroupIOBinding.cs` | 自定義 I/O 綁定邏輯 |

#### 7.7.2 群組節點類型

**1. 標準群組節點 (GroupNodeViewModel)**

使用預設的 `NodeView` 樣式，外觀與一般節點相似。

```
右鍵選單 → 「群組選取節點」
```

**2. 容器樣式群組節點 (ContainerGroupNodeViewModel)**

使用自定義的 `GroupNodeView`，具有 Header 容器設計，內部以網格佈局顯示被群組化節點的產品圖片和名稱。

```
右鍵選單 → 「群組選取節點 (容器樣式)」
```

**視覺結構:**
```
┌─────────────────────────────────────────┐
│  Group (3 nodes)                    [⌄] │  ← Header (深色背景)
├─────────────────────────────────────────┤
│  Grouped Nodes:                         │
│  ┌────────┐  ┌────────┐  ┌────────┐    │
│  │  [圖]  │  │  [圖]  │  │  [圖]  │    │  ← 網格佈局
│  │  TP1   │  │  TBB2  │  │  TC5   │    │
│  └────────┘  └────────┘  └────────┘    │
│                                         │
│  ○ In                          Out ○   │  ← 代理端口
└─────────────────────────────────────────┘
```

#### 7.7.3 ContainerGroupNodeViewModel 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Subnet` | `NetworkViewModel` | 群組內部的子網路 |
| `IOBinding` | `NodeGroupIOBinding` | I/O 綁定物件 |
| `SubnetNodePreviewsBinding` | `ReadOnlyObservableCollection<SubnetNodePreview>` | 子節點預覽列表 |
| `HeaderColor` | `Color?` | Header 背景顏色 |
| `BorderColor` | `Color?` | 容器邊框顏色 |
| `IsExpanded` | `bool` | 內容區域展開狀態 |

**SubnetNodePreview 類別:**
```csharp
public class SubnetNodePreview
{
    public string Name { get; set; }           // 節點名稱
    public ImageSource Image { get; set; }     // 產品圖片
    public NodeType NodeType { get; set; }     // 節點類型
    public string ModelNumber { get; set; }    // 產品型號
}
```

#### 7.7.4 端口映射規則

| 情境 | Group Node 端口 | 端口位置 | 命名 |
|------|----------------|----------|------|
| 外部 Output → 群組內部 Input | Input 端口 | 左側 | "In" |
| 群組內部 Output → 外部 Input | Output 端口 | 右側 | "Out" |

#### 7.7.5 特性

- **類型保留**: 代理端口會保留原始連線端口的 `PortType`
- **自動清理**: 當連接到代理端口的連線被移除時，代理端口自動消失
- **拖曳支援**: 容器樣式群組節點使用 `NodeView` 作為基礎，保留完整拖曳功能
- **子節點預覽**: 容器內以網格佈局顯示被群組化節點的產品圖片和名稱

#### 7.7.6 交互操作

| 操作 | 方式 |
|------|------|
| 標準群組化 | 選取多個節點 → 右鍵選單 →「群組選取節點」 |
| 容器群組化 | 選取多個節點 → 右鍵選單 →「群組選取節點 (容器樣式)」 |
| 解散群組 | 選取 Group Node → 右鍵選單 →「解散群組」 |
| 進入群組 | 選取 Group Node → 右鍵選單 →「進入群組」(或雙擊) |

#### 7.7.7 實作細節

**GroupNodeView 使用 NodeView 作為基礎:**
```xaml
<views:NodeView x:Name="NodeView">
    <views:NodeView.LeadingControlPresenterStyle>
        <Style TargetType="ContentPresenter">
            <Setter Property="ContentTemplate">
                <Setter.Value>
                    <DataTemplate DataType="{x:Type nodes:ContainerGroupNodeViewModel}">
                        <!-- 子節點網格佈局 -->
                        <ItemsControl ItemsSource="{Binding SubnetNodePreviewsBinding}">
                            <ItemsControl.ItemsPanel>
                                <ItemsPanelTemplate>
                                    <WrapPanel Orientation="Horizontal"/>
                                </ItemsPanelTemplate>
                            </ItemsControl.ItemsPanel>
                        </ItemsControl>
                    </DataTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </views:NodeView.LeadingControlPresenterStyle>
</views:NodeView>
```

**群組化時收集子節點預覽:**
```csharp
// MainViewModel.cs - GroupNodesAsContainerCommand
var binding = _containerGrouper.MergeIntoGroup(Network, selectedNodes);
if (binding.GroupNode is ContainerGroupNodeViewModel containerGroupVm)
{
    containerGroupVm.IOBinding = binding;
    containerGroupVm.CollectNodePreviews(selectedNodes);
    containerGroupVm.Name = $"Group ({selectedNodes.Count} nodes)";
}
```

### 7.8 視覺分組機制 (FrameNode)

定義於 `ActuatorApp/ViewModels/Nodes/FrameNodeViewModel.cs`:

**用途:** 提供類似 Blender Geometry Nodes 的視覺分組容器，用於標註和組織節點，但不改變節點結構。

#### 7.8.1 核心元件

| 元件 | 檔案位置 | 說明 |
|------|----------|------|
| `FrameNodeViewModel` | `Nodes/FrameNodeViewModel.cs` | Frame 的 ViewModel |
| `FrameNodeView` | `Views/FrameNodeView.xaml` | Frame 的視覺呈現 |

#### 7.8.2 視覺結構

```
┌─ Frame (2 nodes) ────────────────────────────┐
│                                              │
│   ┌─────────┐       ┌─────────┐              │
│   │  TBB4   │──────▶│  TC14   │──────────────┼──▶ (連線穿過邊界)
│   └─────────┘       └─────────┘              │
│                                              │
└──────────────────────────────────────────────┘
```

#### 7.8.3 FrameNodeViewModel 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Name` | `string` | Frame 標題 |
| `Position` | `Point` | 左上角位置 |
| `Size` | `Size` | Frame 尺寸 |
| `BorderColor` | `Color` | 邊框顏色 |
| `BackgroundColor` | `Color` | 背景顏色 (半透明) |
| `IsSelected` | `bool` | 選取狀態 (顯示黃色邊框) |
| `ContainedNodes` | `IObservableList<NodeViewModel>` | 包含的節點列表 |

#### 7.8.4 交互操作

| 操作 | 方式 |
|------|------|
| 建立 Frame | 選取節點 → 右鍵選單 →「建立 Frame (視覺分組)」 |
| 拖曳 Frame | 拖曳 Header 區域 → Frame 和內部節點一起移動 |
| 取消選取 | 點擊畫布空白區域或其他節點 |

#### 7.8.5 Frame vs GroupNode 對比

| 特點 | FrameNode | GroupNode |
|------|-----------|-----------|
| **用途** | 視覺組織/標註 | 功能性封裝 |
| **內部節點** | 完整顯示 | 隱藏 |
| **連線** | 直接穿過邊界 | 使用代理端口 |
| **編輯** | 直接操作 | 需「進入群組」 |
| **右鍵選單** | 「建立 Frame」 | 「群組選取節點」 |

#### 7.8.6 實作細節

**Frame 注入到 NetworkView 內部:**
```csharp
// MainWindow.xaml.cs - InjectFramesIntoNetworkView()
var contentContainer = FindChild<Canvas>(NetworkView, "contentContainer");
contentContainer.Children.Insert(1, FramesControl);
```

**拖曳使用 Preview 事件:**
```csharp
// FrameNodeView.xaml.cs - 使用隧道事件優先於 DragCanvas
HeaderBorder.PreviewMouseLeftButtonDown += OnHeaderMouseDown;
HeaderBorder.PreviewMouseMove += OnHeaderMouseMove;
HeaderBorder.PreviewMouseLeftButtonUp += OnHeaderMouseUp;
```

## 8. 節點圖片顯示

### 8.1 功能描述
在節點標題下方、端口列表上方顯示產品圖片，以提供直觀的視覺識別。

### 8.2 實作方式
使用 `NodeView.LeadingControlPresenterStyle` 將圖片注入到節點的標準佈局中，避免使用 `Grid` 疊加導致遮擋端口名稱。

```xaml
<views:NodeView.LeadingControlPresenterStyle>
    <Style TargetType="ContentPresenter">
        <Setter Property="Content">
            <Setter.Value>
                <Border>
                    <Image Source="{Binding ProductImageSource, ElementName=Root}" ... />
                </Border>
            </Setter.Value>
        </Setter>
    </Style>
</views:NodeView.LeadingControlPresenterStyle>
```

### 8.3 資料綁定
- **ViewModel**: `ActuatorNodeViewModel` 包含 `ProductImage` 屬性 (ImageSource)。
- **View Code-behind**: 將 ViewModel 的 `ProductImage` 綁定到 View 的 Dependency Property `ProductImageSource`。
- **XAML**: `Image` 控制項綁定到 `ProductImageSource`，並使用 `NullToVisibilityConverter` 在無圖片時隱藏區域。

---

## 9. 視覺設計

### 9.1 端口外觀
| PortType | 形狀 | 顏色 | 邊框 |
|----------|------|------|------|
| Power | 圓形 | #2196F3 (藍色) | #1565C0 |
| BinaryData | 方形 | #607D8B (灰色) | #455A64 |
| Parameter | 菱形 (45°旋轉方形) | #FFC107 (黃色) | #FF8F00 |

### 9.2 節點背景顏色
| NodeType | 顏色 | 色碼 |
|----------|------|------|
| PowerSupply | 綠色 | #4CAF50 |
| Battery | 淺綠 | #8BC34A |
| ControlBox | 藍色 | #2196F3 |
| Actuator | 橙色 | #FF9800 |
| Control | 紫色 | #9C27B0 |
| Accessory | 灰色 | #607D8B |
| Parameter | 黃色 | #FFC107 |

### 9.3 節點結構
- 標題列：節點名稱 (型號)
- 內容區：產品圖片 (預留)
- 端口：左側 Input，右側 Output

---

## 10. 設計決策記錄

| 項目 | 決策 | 說明 |
|------|------|------|
| 參數節點互動方式 | 連線 + 編輯器雙模式 | 預設顯示編輯器，也可接收外部參數節點 |
| TC M端口數量 | 動態配置 | 由後臺提供對應的屬性與參數 |
| TH/TFH 通訊協定 | 單選 | GPIO / Scan / TBUS 只能選擇一種 |
| `a` 端口 | 保留不實作 | 代表 additional component，未來擴充用 |
| 連線驗證 | 依電流/信號流向 | TP → TBB → TC → TA/TH/Accessories |

---

## 11. 專案檔案結構

```
📁 NodeNetwork/
├── 📁 ActuatorApp.Core/           ★ 核心層 (無 UI 依賴, netstandard2.0)
│   ├── ActuatorApp.Core.csproj
│   ├── 📁 Enums/
│   │   ├── NodeType.cs
│   │   ├── PortType.cs
│   │   └── ProtocolType.cs
│   ├── 📁 Interfaces/
│   │   ├── IActuatorNode.cs
│   │   ├── IActuatorPort.cs
│   │   ├── IConnectionValidator.cs
│   │   ├── INodeFactory.cs
│   │   └── IProductRepository.cs      ★ 新增：資料存取介面
│   ├── 📁 Products/
│   │   ├── CategoryDefinition.cs
│   │   ├── PortDefinition.cs
│   │   ├── ProductCatalog.cs
│   │   └── ProductDefinition.cs
│   ├── 📁 Entities/                    ★ 新增：資料庫實體
│   │   └── ProductEntities.cs
│   └── 📁 Validation/
│       └── ConnectionRuleEngine.cs
│
├── 📁 ActuatorApp.Infrastructure/      ★ 新增：基礎設施層
│   ├── ActuatorApp.Infrastructure.csproj
│   └── 📁 Repositories/
│       └── ProductRepository.cs
│
├── 📁 ActuatorApp/                 ★ UI 層 (NodeNetwork 實作, netcoreapp3.1)
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── ActuatorApp.csproj
│   ├── 📁 Adapters/                ★ 適配器層
│   │   └── NodeNetworkNodeFactory.cs
│   ├── 📁 Services/                ★ 新增：應用服務
│   │   ├── IAutoConnectService.cs
│   │   └── AutoConnectService.cs
│   ├── 📁 Properties/
│   │   └── AssemblyInfo.cs
│   ├── 📁 Models/
│   │   ├── PowerSignal.cs
│   │   ├── BinarySignal.cs
│   │   └── ParameterValue.cs
│   ├── 📁 ViewModels/
│   │   ├── ActuatorPortViewModel.cs
│   │   ├── ActuatorNodeViewModel.cs    ★ 實作 IActuatorNode
│   │   ├── ActuatorInputViewModel.cs
│   │   ├── ActuatorOutputViewModel.cs
│   │   ├── MainViewModel.cs            ★ 使用 ProductCatalog
│   │   ├── 📁 Nodes/
│   │   │   ├── TPNodeViewModel.cs
│   │   │   ├── TBBNodeViewModel.cs
│   │   │   ├── TCNodeViewModel.cs
│   │   │   ├── TANodeViewModel.cs
│   │   │   ├── THNodeViewModel.cs
│   │   │   ├── TYCNodeViewModel.cs
│   │   │   ├── THPNodeViewModel.cs
│   │   │   ├── ParameterNodeViewModel.cs
│   │   │   └── GroupNodeViewModel.cs   ★ 新增：群組節點
│   │   └── 📁 Editors/
│   │       └── ParameterValueEditorViewModel.cs
│   ├── 📁 Views/
│   │   ├── ActuatorPortView.xaml
│   │   ├── ActuatorPortView.xaml.cs
│   │   ├── ActuatorNodeView.xaml
│   │   ├── ActuatorNodeView.xaml.cs
│   │   ├── ParameterNodeView.xaml
│   │   ├── ParameterNodeView.xaml.cs
│   │   ├── THNodeView.xaml
│   │   ├── THNodeView.xaml.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── 📁 Converters/              ★ 新增：值轉換器
│   │   │   └── (XAML 轉換器)
│   │   └── 📁 Editors/
│   │       ├── ParameterValueEditorView.xaml
│   │       └── ParameterValueEditorView.xaml.cs
│   ├── 📁 Validation/
│   │   └── ConnectionRules.cs
│   └── 📁 Resources/
│       └── (產品圖片檔案 - 預留)
```

---

## 12. 相依套件

### ActuatorApp.Core (netstandard2.0)
```xml
<!-- 無外部相依，僅使用 .NET Standard 內建函式庫 -->
<PackageReference Include="System.Text.Json" Version="6.0.0" />
```

### ActuatorApp.Infrastructure (netstandard2.0)
```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="7.0.0" />
<PackageReference Include="Dapper" Version="2.0.123" />
<ProjectReference Include="..\ActuatorApp.Core\ActuatorApp.Core.csproj" />
```

### ActuatorApp (netcoreapp3.1)
```xml
<PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
<PackageReference Include="ReactiveUI" Version="13.2.18" />
<PackageReference Include="ReactiveUI.WPF" Version="13.2.18" />
<ProjectReference Include="..\ActuatorApp.Core\ActuatorApp.Core.csproj" />
<ProjectReference Include="..\ActuatorApp.Infrastructure\ActuatorApp.Infrastructure.csproj" />
<ProjectReference Include="..\NodeNetwork\NodeNetwork.csproj" />
<ProjectReference Include="..\NodeNetworkToolkit\NodeNetworkToolkit.csproj" />
```

---

## 13. 遷移指南 (從 NodeNetwork 到其他框架)

如需遷移至其他節點編輯器框架（如付費套件），請依照以下步驟：

### 12.1 Core 層 (無需修改)
`ActuatorApp.Core` 專案完全不依賴 NodeNetwork，可直接引用。

### 12.2 需重新實作的部分
1. **建立新的 NodeFactory** - 實作 `INodeFactory` 介面
2. **建立新的 Node ViewModels** - 實作 `IActuatorNode` 介面
3. **建立新的 Port ViewModels** - 實作 `IActuatorPort` 介面
4. **建立新的 ConnectionValidator** - 使用 `ConnectionRuleEngine` 的純邏輯

### 12.3 可重用的部分
- `ProductCatalog` - 所有產品定義
- `ConnectionRuleEngine` - 連線驗證邏輯
- `Enums` - NodeType, PortType, ProtocolType
- JSON 產品配置檔案

### 12.4 範例：遷移至假設的 "SuperNodeEditor" 框架
```csharp
// 新的 NodeFactory
public class SuperNodeEditorFactory : INodeFactory
{
    private readonly ProductCatalog _catalog;
    
    public object CreateNode(ProductDefinition product)
    {
        // 使用 SuperNodeEditor 的 API 建立節點
        var node = new SuperNode();
        node.Name = product.ModelNumber;
        // ... 設定端口等
        return node;
    }
}
```

---

## 14. 使用說明

### 13.1 啟動專案
1. 開啟 Visual Studio
2. 載入 `NodeNetwork.sln` 方案
3. 將 `ActuatorApp` 設為啟動專案
4. 按 F5 執行

### 13.2 新增節點
1. 從左側 TabControl 選擇產品分類
2. 點擊節點項目將其加入畫布
3. 拖曳節點調整位置

### 13.3 建立連線
1. 從輸出端口拖曳至輸入端口
2. 系統會自動驗證連線是否有效
3. 無效連線會顯示錯誤訊息

### 13.4 參數設定
1. 點擊「新增參數節點」按鈕
2. 在黃色橢圓節點中輸入參數值
3. 將參數節點連接到 TA 的 Code 或 Stroke 端口

---

## 15. 後續擴充項目

- [ ] 產品圖片載入機制
- [ ] 後臺配置 API 整合
- [ ] Get ID 按鈕功能實作
- [ ] `a` 端口功能定義
- [ ] 序列化/反序列化 (儲存/載入配置)
- [ ] 配置驗證 (完整性檢查)
- [ ] 匯出報表功能
- [ ] 拖放 (Drag & Drop) 節點支援
- [ ] 節點複製/貼上功能
- [ ] Undo/Redo 功能
- [ ] JSON 產品配置外部檔案載入
- [ ] 多語系支援

---

## 16. 參考資料

- [NodeNetwork GitHub](https://github.com/Wouterdek/NodeNetwork)
- [NodeNetwork 文件](https://wouterdek.github.io/NodeNetwork/doc)
- [ReactiveUI 文件](https://www.reactiveui.net/)
- [Bilibili NodeNetwork 介紹](https://www.bilibili.com/opus/774615068804382772)