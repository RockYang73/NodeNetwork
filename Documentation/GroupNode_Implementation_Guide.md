# NodeNetwork 群組節點實作指南 (Group Node Implementation Guide)

## 1. 問題描述 (Problem Statement)

使用者希望評估 `NodeNetwork` 專案是否能夠支援「群組節點 (Group Node)」功能，具體要求如下：
1.  **巢狀結構**: 群組節點能夠包含多個子節點。
2.  **連接能力**: 群組節點本身能夠與其他群組節點或普通節點進行連接。
3.  **連通性限制**: 僅允許將已連線的節點群組化，避免將多個互不相連的節點區塊合併成一個群組。

## 2. 架構評估 (Architectural Evaluation)

經分析 `NodeNetwork` 程式碼庫，尤其是 `NodeNetworkToolkit` 中的 `NodeNetwork.Toolkit.Group` 命名空間，確認此功能已具備核心支援。

*   **核心支援**: `NodeNetworkToolkit` 專為節點群組化提供了 `NodeGrouper` 類別，它能將選定的節點集合移動到一個新的子網路 (sub-network) 中，並在父網路中建立一個代表該子網路的單一節點 (即群組節點)。
*   **巢狀結構 (Nesting)**: `NodeGrouper` 透過 `SubNetworkFactory` 建立一個 `NetworkViewModel` 來容納子節點。因此，群組節點實際上是一個包含 `NetworkViewModel` 的 `NodeViewModel`。由於群組節點本身也是一個 `NodeViewModel`，理論上可以實現多層次的巢狀群組。
*   **連接能力 (Connectivity)**:
    *   **對外連接**: 群組節點在父網路中是一個普通的 `NodeViewModel`，因此它具備 `Inputs` 和 `Outputs`。`NodeGrouper` 會自動在群組節點上創建對應的端口，以處理跨越群組邊界的連線。
    *   **對內連接**: 群組內部的節點通過在子網路中自動生成的 `EntranceNode` 和 `ExitNode` 與群組節點的外部端口進行數據傳遞。

## 3. 關鍵類別 (Key Classes)

以下是實作群組節點功能所需的關鍵類別，它們位於 `D:\workspace\NodeNetwork\NodeNetworkToolkit\Group\`：

*   **`NodeGrouper.cs`**:
    *   **職責**: 這是執行群組化操作的核心邏輯。它負責創建子網路、群組節點、內部入口/出口節點，以及處理節點的移動和連線的重新配置。
    *   **重要屬性**:
        *   `GroupNodeFactory`: 用於創建代表群組的 `NodeViewModel`。
        *   `SubNetworkFactory`: 用於創建容納子節點的 `NetworkViewModel`。
        *   `EntranceNodeFactory`, `ExitNodeFactory`: 用於創建子網路內部與外部連線溝通的節點。
        *   `IOBindingFactory`: 用於建立群組節點外部端口與內部入口/出口節點之間的綁定。
    *   **關鍵方法**:
        *   `MergeIntoGroup(NetworkViewModel network, IEnumerable<NodeViewModel> nodesToGroup)`: 將選定的節點群組化。
        *   `Ungroup(NodeGroupIOBinding nodeGroupInfo)`: 解除群組。

*   **`NodeGroupIOBinding.cs`**:
    *   **職責**: 定義了群組節點 (父網路) 的輸入/輸出與子網路內部 `EntranceNode`/`ExitNode` 之間如何映射和綁定的抽象。具體實作通常是 `ValueNodeGroupIOBinding`。

*   **`GraphAlgorithms.cs`**:
    *   **職責**: 包含各種圖形演算法，其中 `FindSubGraphs` 對於實現連通性限制至關重要。
    *   **關鍵方法**: `FindSubGraphs(IEnumerable<NodeViewModel> nodes)`: 將一個節點集合根據連線關係拆分成多個連通的子圖。

## 4. 實作細節與建議 (Implementation Details and Recommendations)

為了實現群組節點功能並加入連通性限制，您需要進行以下步驟：

### 4.1 建立自定義群組節點 ViewModel (Custom Group Node ViewModel)

為了讓群組節點能持有其子網路的參考，建議繼承 `NodeViewModel` 建立一個自定義類別：

**檔案**: `ActuatorApp/ViewModels/Nodes/GroupNodeViewModel.cs` (或您專案中合適的位置)
```csharp
using NodeNetwork.ViewModels;

public class GroupNodeViewModel : NodeViewModel
{
    /// <summary>
    /// 存放該群組節點所包含的子網路。
    /// </summary>
    public NetworkViewModel Subnet { get; }

    public GroupNodeViewModel(NetworkViewModel subnet)
    {
        this.Subnet = subnet;
        this.Name = "Group Node"; // 可自行設定預設名稱
        // 可以在此處設定其他群組節點特有的屬性或外觀
    }
}
```

### 4.2 配置 `NodeGrouper` (Configuring `NodeGrouper`)

在使用 `NodeGrouper` 進行群組化操作前，需要配置其工廠方法，使其使用您的自定義 `GroupNodeViewModel`：

```csharp
// 在需要執行群組化操作的地方，例如您的 MainViewModel 或某個命令處理器
var grouper = new NodeGrouper
{
    // 關鍵設定：讓 Grouper 使用您的 GroupNodeViewModel，並傳入 subnet
    GroupNodeFactory = (subnet) => new GroupNodeViewModel(subnet),
    
    // 定義如何創建新的子網路實例
    SubNetworkFactory = () => new NetworkViewModel(),
    
    // 定義子網路內部的入口節點 (通常處理外部進入群組的連線)
    EntranceNodeFactory = () => new NodeViewModel { Name = "Group Input" }, 
    
    // 定義子網路內部的出口節點 (通常處理群組內部連到外部的連線)
    ExitNodeFactory = () => new NodeViewModel { Name = "Group Output" },
    
    // 使用預設的 IO 綁定 (通常是 ValueNodeGroupIOBinding)
    IOBindingFactory = (groupNode, entranceNode, exitNode) => 
        new ValueNodeGroupIOBinding(groupNode, entranceNode, exitNode)
};
```

### 4.3 執行群組化操作 (Executing Grouping)

一旦 `NodeGrouper` 配置完成，您就可以呼叫 `MergeIntoGroup` 方法來將選定的節點群組化：

```csharp
// 假設 `network` 是當前操作的 NetworkViewModel，`SelectedNodes` 是使用者選中的節點集合
var nodesToGroup = network.SelectedNodes.Items; // 確保這是 System.Collections.Generic.IEnumerable<NodeViewModel> 類型

if (nodesToGroup.Any())
{
    grouper.MergeIntoGroup(network, nodesToGroup);
}
```

### 4.4 新增連通性限制 (Adding Connectivity Restriction)

為了確保只有彼此連線的節點才能被群組化，您需要在呼叫 `MergeIntoGroup` 之前增加一個檢查。這會修改 `NodeGrouper.cs` 中的 `MergeIntoGroup` 方法。

**修改 `D:\workspace\NodeNetwork\NodeNetworkToolkit\Group\NodeGrouper.cs`：**

**定位程式碼段落：**
在 `NodeGrouper.cs` 檔案的 `MergeIntoGroup` 方法中，找到以下現有檢查：
```csharp
// Check if nodesToGroup can be combined into a single group
if (groupNodesSet.Count == 0 || !GraphAlgorithms.IsContinuousSubGraphSet(groupNodesSet))
{
    return null;
}
```

**新增連通性檢查 (插入在上述程式碼之後)：**
```csharp
// Check if nodesToGroup can be combined into a single group
if (groupNodesSet.Count == 0 || !GraphAlgorithms.IsContinuousSubGraphSet(groupNodesSet))
{
    return null;
}

// [新增] 連通性限制：判斷選定的節點是否形成單一的連通子圖。
// 如果選取的節點被 GraphAlgorithms.FindSubGraphs 分成了多於 1 個子圖，
// 則表示它們之間存在斷開，不符合「只能群組已連線節點」的要求。
if (GraphAlgorithms.FindSubGraphs(groupNodesSet).Count() > 1)
{
    // 您可以在此處拋出一個更具體的異常，或返回 null，
    // 以通知呼叫方群組操作失敗的原因。
    // 例如：throw new InvalidOperationException("只能群組彼此連線的節點。");
    return null; 
}

// ... 後續的群組化邏輯 ...
```

**說明**:
*   `GraphAlgorithms.FindSubGraphs(nodesToGroup)` 會根據節點間的連線關係，將 `nodesToGroup` 集合劃分為數個「連通子圖」。
*   如果返回的子圖數量大於 1，則表示使用者選擇的節點中有不相連的區塊，此時應阻止群組化操作。

## 5. 總結

`NodeNetwork` 提供了一個強大且彈性的基礎框架來實作群組節點。透過適當配置 `NodeGrouper` 的工廠方法並引入自定義 `GroupNodeViewModel`，您可以輕鬆實現巢狀和可連接的群組節點。同時，利用 `GraphAlgorithms` 中的輔助方法，可以進一步對群組化操作施加連通性等邏輯限制，以滿足特定的應用需求。
