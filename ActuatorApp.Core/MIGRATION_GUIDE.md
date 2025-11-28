# ActuatorApp.Core 遷移指南

## 文件資訊
| 項目 | 內容 |
|------|------|
| **版本** | 1.0 |
| **建立日期** | 2025年11月27日 |
| **適用對象** | 需要將 ActuatorApp 遷移至其他節點編輯器框架的開發人員 |

---

## 1. 架構概述

### 1.1 分層架構

```
┌─────────────────────────────────────────────────────────────┐
│                    UI Layer (可替換)                         │
│         ActuatorApp / 其他節點編輯器實作專案                  │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Adapter Layer (適配器層)                │    │
│  │  - NodeNetworkNodeFactory                           │    │
│  │  - 實作 INodeFactory                                │    │
│  │  - 將 ProductDefinition 轉換為框架節點              │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ depends on
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              ActuatorApp.Core (核心層 - 不可替換)            │
│                     netstandard2.0                          │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐        │
│  │  Interfaces  │ │   Products   │ │  Validation  │        │
│  │              │ │              │ │              │        │
│  │ IActuatorNode│ │ProductCatalog│ │ConnectionRule│        │
│  │ IActuatorPort│ │ProductDefini-│ │   Engine     │        │
│  │ INodeFactory │ │    tion      │ │              │        │
│  │IConnection-  │ │CategoryDefin-│ │              │        │
│  │  Validator   │ │    ition     │ │              │        │
│  └──────────────┘ └──────────────┘ └──────────────┘        │
│                                                             │
│  ┌──────────────────────────────────────────────────┐      │
│  │                      Enums                        │      │
│  │  NodeType | PortType | ProtocolType               │      │
│  └──────────────────────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 設計原則

- **ActuatorApp.Core** 完全不依賴任何 UI 框架
- 所有產品定義、商業邏輯、驗證規則都在 Core 層
- UI 層透過實作介面來適配不同的節點編輯器框架

---

## 2. 遷移前準備

### 2.1 理解核心介面

#### IActuatorNode
```csharp
public interface IActuatorNode
{
    NodeType NodeType { get; }
    string ModelNumber { get; }
    string DisplayName { get; }
}
```

#### IActuatorPort
```csharp
public interface IActuatorPort
{
    PortType PortType { get; }
    string Name { get; }
    IActuatorNode ParentNode { get; }
}
```

#### INodeFactory
```csharp
public interface INodeFactory
{
    object CreateNode(ProductDefinition product);
    object CreateNodeByModel(string modelNumber);
}
```

#### IConnectionValidator
```csharp
public interface IConnectionValidator
{
    ConnectionValidationResult Validate(
        IActuatorPort sourcePort, 
        IActuatorPort targetPort);
}
```

### 2.2 可重用的資源

| 資源 | 說明 | 位置 |
|------|------|------|
| ProductCatalog | 30+ 產品定義 | `Products/ProductCatalog.cs` |
| ConnectionRuleEngine | 連線驗證邏輯 | `Validation/ConnectionRuleEngine.cs` |
| Enums | 所有枚舉定義 | `Enums/` |
| JSON 載入 | 支援從 JSON 載入產品 | `ProductCatalog.LoadFromJson()` |

---

## 3. 遷移步驟

### Step 1: 建立新專案

```bash
# 建立新的 UI 專案
dotnet new wpf -n ActuatorApp.NewFramework
cd ActuatorApp.NewFramework

# 加入 Core 參考
dotnet add reference ../ActuatorApp.Core/ActuatorApp.Core.csproj

# 加入新框架的 NuGet 套件
dotnet add package NewNodeEditorFramework
```

### Step 2: 實作節點基類

建立一個實作 `IActuatorNode` 的節點基類：

```csharp
using ActuatorApp.Core.Interfaces;
using ActuatorApp.Core.Enums;
using NewNodeEditorFramework; // 新框架的命名空間

namespace ActuatorApp.NewFramework.ViewModels
{
    public abstract class ActuatorNodeBase : NewFrameworkNodeBase, IActuatorNode
    {
        public NodeType NodeType { get; }
        public string ModelNumber { get; set; }
        public string DisplayName => Name; // 假設新框架有 Name 屬性

        protected ActuatorNodeBase(NodeType nodeType)
        {
            NodeType = nodeType;
        }
    }
}
```

### Step 3: 實作端口類別

```csharp
using ActuatorApp.Core.Interfaces;
using ActuatorApp.Core.Enums;

namespace ActuatorApp.NewFramework.ViewModels
{
    public class ActuatorPort : NewFrameworkPortBase, IActuatorPort
    {
        public PortType PortType { get; }
        public string Name { get; set; }
        public IActuatorNode ParentNode { get; }

        public ActuatorPort(PortType portType, IActuatorNode parent)
        {
            PortType = portType;
            ParentNode = parent;
        }
    }
}
```

### Step 4: 實作 NodeFactory

```csharp
using ActuatorApp.Core.Interfaces;
using ActuatorApp.Core.Products;
using ActuatorApp.Core.Enums;

namespace ActuatorApp.NewFramework.Adapters
{
    public class NewFrameworkNodeFactory : INodeFactory
    {
        private readonly ProductCatalog _catalog;

        public NewFrameworkNodeFactory(ProductCatalog catalog)
        {
            _catalog = catalog;
        }

        public object CreateNode(ProductDefinition product)
        {
            return product.NodeType switch
            {
                NodeType.PowerSupply => CreatePowerSupplyNode(product),
                NodeType.Battery => CreateBatteryNode(product),
                NodeType.ControlBox => CreateControlBoxNode(product),
                NodeType.Actuator => CreateActuatorNode(product),
                NodeType.Control => CreateControlNode(product),
                NodeType.Accessory => CreateAccessoryNode(product),
                _ => throw new NotSupportedException()
            };
        }

        public object CreateNodeByModel(string modelNumber)
        {
            var product = _catalog.GetProduct(modelNumber);
            return CreateNode(product);
        }

        private object CreatePowerSupplyNode(ProductDefinition product)
        {
            var node = new TPNode();
            node.ModelNumber = product.ModelNumber;
            
            // 根據 product.Outputs 建立端口
            foreach (var portDef in product.Outputs)
            {
                node.AddOutput(new ActuatorPort(portDef.PortType, node)
                {
                    Name = portDef.Name
                });
            }
            
            return node;
        }

        // ... 其他節點類型的建立方法
    }
}
```

### Step 5: 整合連線驗證

```csharp
using ActuatorApp.Core.Validation;

namespace ActuatorApp.NewFramework
{
    public class ConnectionValidator
    {
        private readonly ConnectionRuleEngine _ruleEngine;

        public ConnectionValidator()
        {
            _ruleEngine = new ConnectionRuleEngine();
        }

        // 在新框架的連線驗證回調中呼叫
        public bool ValidateConnection(IActuatorPort source, IActuatorPort target)
        {
            var result = _ruleEngine.ValidateConnection(
                source.PortType, 
                source.ParentNode.NodeType,
                target.PortType, 
                target.ParentNode.NodeType);
            
            return result.IsValid;
        }
    }
}
```

### Step 6: 建立主 ViewModel

```csharp
using ActuatorApp.Core.Products;

namespace ActuatorApp.NewFramework.ViewModels
{
    public class MainViewModel
    {
        private readonly ProductCatalog _catalog;
        private readonly NewFrameworkNodeFactory _factory;

        public MainViewModel()
        {
            // 從 Core 層載入產品目錄
            _catalog = ProductCatalog.CreateDefault();
            _factory = new NewFrameworkNodeFactory(_catalog);

            // 根據 Catalog 建立 UI 分類
            BuildCategories();
        }

        private void BuildCategories()
        {
            foreach (var category in _catalog.Categories)
            {
                // 建立 UI 分類...
                foreach (var product in category.Products)
                {
                    // 註冊節點建立工廠
                    RegisterNodeFactory(product.ModelNumber, 
                        () => _factory.CreateNodeByModel(product.ModelNumber));
                }
            }
        }
    }
}
```

---

## 4. 各節點實作範例

### 4.1 TP 節點 (Power Supply)

```csharp
public class TPNode : ActuatorNodeBase
{
    public ActuatorPort PowerOut { get; }

    public TPNode(string model) : base(NodeType.PowerSupply)
    {
        ModelNumber = model;
        Name = model;

        PowerOut = new ActuatorPort(PortType.Power, this)
        {
            Name = "Out"
        };
        AddOutput(PowerOut);
    }
}
```

### 4.2 TC 節點 (Control Box) - 動態端口

```csharp
public class TCNode : ActuatorNodeBase
{
    public ActuatorPort PowerIn { get; }
    public ActuatorPort H1In { get; }
    public List<ActuatorPort> MotorOutputs { get; } = new();

    public TCNode(ProductDefinition product) : base(NodeType.ControlBox)
    {
        ModelNumber = product.ModelNumber;
        Name = product.ModelNumber;

        // 電源輸入
        PowerIn = new ActuatorPort(PortType.Power, this) { Name = "Power" };
        AddInput(PowerIn);

        // 手控器輸入
        H1In = new ActuatorPort(PortType.BinaryData, this) { Name = "H1" };
        AddInput(H1In);

        // 動態建立 M 端口
        int motorCount = product.Properties.ContainsKey("MotorCount") 
            ? (int)product.Properties["MotorCount"] 
            : 3;

        for (int i = 1; i <= motorCount; i++)
        {
            var motor = new ActuatorPort(PortType.BinaryData, this) 
            { 
                Name = $"M{i}" 
            };
            MotorOutputs.Add(motor);
            AddOutput(motor);
        }
    }
}
```

### 4.3 TH 節點 (Control) - 帶屬性

```csharp
public class THNode : ActuatorNodeBase
{
    public ActuatorPort TCOut { get; }
    
    // 從 ProductDefinition.Properties 讀取支援的協定
    public ProtocolType SelectedProtocol { get; set; } = ProtocolType.GPIO;

    public THNode(ProductDefinition product) : base(NodeType.Control)
    {
        ModelNumber = product.ModelNumber;
        Name = product.ModelNumber;

        TCOut = new ActuatorPort(PortType.BinaryData, this) { Name = "TC" };
        AddOutput(TCOut);
    }
}
```

---

## 5. JSON 產品配置

### 5.1 JSON 格式範例

```json
{
  "Products": [
    {
      "ModelNumber": "TP5",
      "NodeType": "PowerSupply",
      "Category": "Power Supply",
      "BackgroundColor": "#4CAF50",
      "Outputs": [
        { "Name": "Out", "PortType": "Power" }
      ]
    },
    {
      "ModelNumber": "TC14",
      "NodeType": "ControlBox",
      "Category": "Control Box",
      "BackgroundColor": "#2196F3",
      "Properties": {
        "MotorCount": 3
      },
      "Inputs": [
        { "Name": "Power", "PortType": "Power" },
        { "Name": "H1", "PortType": "BinaryData" }
      ],
      "Outputs": [
        { "Name": "M", "PortType": "BinaryData", "IsDynamic": true, "Count": 3 }
      ]
    }
  ],
  "Categories": [
    {
      "Name": "Power Supply",
      "Order": 1
    },
    {
      "Name": "Control Box", 
      "Order": 2
    }
  ]
}
```

### 5.2 載入 JSON 配置

```csharp
// 從檔案載入
var catalog = ProductCatalog.LoadFromJson("products.json");

// 從字串載入 (例如從 API 取得)
var catalog = ProductCatalog.LoadFromJsonString(jsonString);
```

---

## 6. 檢查清單

### 遷移前
- [ ] 確認新框架支援自訂節點和端口
- [ ] 確認新框架支援連線驗證回調
- [ ] 閱讀並理解 Core 層的所有介面

### 實作中
- [ ] 實作 `IActuatorNode` 節點基類
- [ ] 實作 `IActuatorPort` 端口類別
- [ ] 實作 `INodeFactory` 工廠類別
- [ ] 整合 `ConnectionRuleEngine` 驗證邏輯
- [ ] 使用 `ProductCatalog` 建立 UI 分類

### 測試
- [ ] 所有節點類型可正確建立
- [ ] 端口連線驗證正確運作
- [ ] Power 流向: TP → TBB → TC
- [ ] BinaryData 流向: TC ↔ TH, TC → TA
- [ ] Parameter 流向: Parameter → TA

---

## 7. 常見問題

### Q1: NodeType 和 PortType 衝突怎麼辦？

使用型別別名：
```csharp
using CoreNodeType = ActuatorApp.Core.Enums.NodeType;
using CorePortType = ActuatorApp.Core.Enums.PortType;
```

### Q2: 如何處理動態端口數量？

從 `ProductDefinition.Properties` 讀取：
```csharp
var motorCount = product.Properties.TryGetValue("MotorCount", out var value)
    ? Convert.ToInt32(value)
    : 3; // 預設值
```

### Q3: 如何擴充新產品？

1. 在 `ProductCatalog.InitializeDefaultProducts()` 新增產品定義
2. 在 `NodeFactory` 新增對應的建立邏輯
3. 或者使用 JSON 配置動態載入

---

## 8. 聯絡資訊

如有任何問題，請參考：
- [NodeNetwork 原始碼](https://github.com/Wouterdek/NodeNetwork)
- `ActuatorApp_Specification.md` 完整規格文件
