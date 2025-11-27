# ActuatorApp 規格文件

## 文件資訊
| 項目 | 內容 |
|------|------|
| **專案名稱** | ActuatorApp (電動推桿控制系統節點編輯器) |
| **基於框架** | NodeNetwork (WPF + ReactiveUI) |
| **建立日期** | 2025年11月27日 |
| **版本** | 1.0 |

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

### 1.3 資料流類型
| 類型 | 說明 | 視覺呈現 | 流向 |
|------|------|----------|------|
| **Power** | 電流傳遞 | 藍色圓形端口 | TP → TBB → TC |
| **BinaryData** | 二進制控制信號 | 灰色方形端口 | TC ↔ TH, TC → TA |
| **Parameter** | 參數值 | 黃色菱形端口 | Parameter → TA |

---

## 2. 產品分類 (NodeGroup)

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

## 6. 視覺設計

### 6.1 端口外觀
| PortType | 形狀 | 顏色 | 邊框 |
|----------|------|------|------|
| Power | 圓形 | #2196F3 (藍色) | #1565C0 |
| BinaryData | 方形 | #607D8B (灰色) | #455A64 |
| Parameter | 菱形 (45°旋轉方形) | #FFC107 (黃色) | #FF8F00 |

### 6.2 節點背景顏色
| NodeType | 顏色 | 色碼 |
|----------|------|------|
| PowerSupply | 綠色 | #4CAF50 |
| Battery | 淺綠 | #8BC34A |
| ControlBox | 藍色 | #2196F3 |
| Actuator | 橙色 | #FF9800 |
| Control | 紫色 | #9C27B0 |
| Accessory | 灰色 | #607D8B |
| Parameter | 黃色 | #FFC107 |

### 6.3 節點結構
- 標題列：節點名稱 (型號)
- 內容區：產品圖片 (預留)
- 端口：左側 Input，右側 Output

---

## 7. 設計決策記錄

| 項目 | 決策 | 說明 |
|------|------|------|
| 參數節點互動方式 | 連線 + 編輯器雙模式 | 預設顯示編輯器，也可接收外部參數節點 |
| TC M端口數量 | 動態配置 | 由後臺提供對應的屬性與參數 |
| TH/TFH 通訊協定 | 單選 | GPIO / Scan / TBUS 只能選擇一種 |
| `a` 端口 | 保留不實作 | 代表 additional component，未來擴充用 |
| 連線驗證 | 依電流/信號流向 | TP → TBB → TC → TA/TH/Accessories |

---

## 8. 專案檔案結構

```
📁 ActuatorApp/
├── App.xaml
├── App.xaml.cs
├── ActuatorApp.csproj
├── 📁 Properties/
│   └── AssemblyInfo.cs
├── 📁 Models/
│   ├── PowerSignal.cs
│   ├── BinarySignal.cs
│   └── ParameterValue.cs
├── 📁 ViewModels/
│   ├── ActuatorPortViewModel.cs
│   ├── ActuatorNodeViewModel.cs
│   ├── ActuatorInputViewModel.cs
│   ├── ActuatorOutputViewModel.cs
│   ├── MainViewModel.cs
│   ├── 📁 Nodes/
│   │   ├── TPNodeViewModel.cs
│   │   ├── TBBNodeViewModel.cs
│   │   ├── TCNodeViewModel.cs
│   │   ├── TANodeViewModel.cs
│   │   ├── THNodeViewModel.cs
│   │   ├── TYCNodeViewModel.cs
│   │   ├── THPNodeViewModel.cs
│   │   └── ParameterNodeViewModel.cs
│   └── 📁 Editors/
│       └── ParameterValueEditorViewModel.cs
├── 📁 Views/
│   ├── ActuatorPortView.xaml
│   ├── ActuatorPortView.xaml.cs
│   ├── ActuatorNodeView.xaml
│   ├── ActuatorNodeView.xaml.cs
│   ├── ParameterNodeView.xaml
│   ├── ParameterNodeView.xaml.cs
│   ├── THNodeView.xaml
│   ├── THNodeView.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── 📁 Editors/
│       ├── ParameterValueEditorView.xaml
│       └── ParameterValueEditorView.xaml.cs
├── 📁 Validation/
│   └── ConnectionRules.cs
└── 📁 Resources/
    └── (產品圖片檔案 - 預留)
```

---

## 9. 相依套件

```xml
<PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
<PackageReference Include="ReactiveUI" Version="13.2.18" />
<PackageReference Include="ReactiveUI.WPF" Version="13.2.18" />
<ProjectReference Include="..\NodeNetwork\NodeNetwork.csproj" />
<ProjectReference Include="..\NodeNetworkToolkit\NodeNetworkToolkit.csproj" />
```

---

## 10. 使用說明

### 10.1 啟動專案
1. 開啟 Visual Studio
2. 載入 `NodeNetwork.sln` 方案
3. 將 `ActuatorApp` 設為啟動專案
4. 按 F5 執行

### 10.2 新增節點
1. 從左側 TabControl 選擇產品分類
2. 點擊節點項目將其加入畫布
3. 拖曳節點調整位置

### 10.3 建立連線
1. 從輸出端口拖曳至輸入端口
2. 系統會自動驗證連線是否有效
3. 無效連線會顯示錯誤訊息

### 10.4 參數設定
1. 點擊「新增參數節點」按鈕
2. 在黃色橢圓節點中輸入參數值
3. 將參數節點連接到 TA 的 Code 或 Stroke 端口

---

## 11. 後續擴充項目

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

---

## 12. 參考資料

- [NodeNetwork GitHub](https://github.com/Wouterdek/NodeNetwork)
- [NodeNetwork 文件](https://wouterdek.github.io/NodeNetwork/doc)
- [ReactiveUI 文件](https://www.reactiveui.net/)
- [Bilibili NodeNetwork 介紹](https://www.bilibili.com/opus/774615068804382772)
