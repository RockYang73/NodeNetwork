# NodeNetwork - 連接驗證邏輯

在 `NodeNetwork` 框架中，節點之間的連接限制是透過 **輸入端點 (Input Endpoint)** 上的 **驗證器 (Validator)** 機制來實現的。簡單來說，就是「**由輸入端決定誰可以連過來**」。

## 1. 核心機制：`ConnectionValidator`

每個輸入端點 (`NodeInputViewModel`) 都擁有一個名為 `ConnectionValidator` 的屬性。這是一個委託 (Delegate) 函式，每當使用者嘗試拖曳連線時，系統便會呼叫它來進行驗證。

```csharp
// 定義在 NodeInputViewModel 中
public Func<PendingConnectionViewModel, ConnectionValidationResult> ConnectionValidator { get; set; }
```

*   **輸入參數**: `PendingConnectionViewModel`，這個物件包含了目前正在嘗試連接的 `Input` 和 `Output` 兩端的詳細資訊。
*   **回傳值**: `ConnectionValidationResult` 物件，其中包含一個 `IsValid` 布林值 (指示連接是否有效)，以及一個可選的 `MessageViewModel` (用於顯示錯誤訊息)。

## 2. 預設行為與限制

### A. 基礎限制 (由核心庫強制執行，無法覆寫)
`NodeNetwork` 核心庫會強制執行以下幾種物理層面的連接限制：

*   **方向限制**: 連接只能從輸出端 (Output) 連接到輸入端 (Input)，不能是輸入端連接輸入端。
*   **同節點限制**: 同一個節點上的輸入端不能直接連接到該節點自身的輸出端，以避免直接的迴路或短路。
*   **數量限制**: 輸入端點的 `MaxConnections` 屬性決定了它可以接受的最大連接數。如果 `MaxConnections` 為 1 (預設值)，則該輸入端點一次只能有一條連線。當有新連線嘗試連接時，它會取代舊有連線，或者直接被拒絕。

### B. 型別安全限制 (NodeNetworkToolkit 的預設行為)
當你使用 `NodeNetworkToolkit` 中的 `ValueNodeInputViewModel<T>` (例如 `ValueNodeInputViewModel<int>`) 時，它在建構函式中已經內建了預設的驗證邏輯：

```csharp
// 在 ValueNodeInputViewModel<T> 的建構函式中
ConnectionValidator = pending => new ConnectionValidationResult(pending.Output is ValueNodeOutputViewModel<T>, null);
```

這段程式碼的意義是：
*   **嚴格型別檢查**: 它會檢查嘗試連接的輸出端是否為相同型別的 `ValueNodeOutputViewModel<T>`。例如，一個 `ValueNodeInputViewModel<int>` 只能連接到 `ValueNodeOutputViewModel<int>`。
*   **繼承檢查**: 實際檢查的是 `pending.Output is ValueNodeOutputViewModel<T>`。這表示如果 `T` 被定義為 `object`，那麼這個輸入端將可以接受任何型別的輸出端。

## 3. 如何自定義連接限制？

如果你需要更靈活或特定的連接規則，例如允許 `int` 型別的輸出連接到 `double` 型別的輸入 (因為 `int` 可以隱式轉換為 `double`)，你可以透過覆寫 `ConnectionValidator` 屬性來實現：

**範例：允許 `int` 連接到 `double`**

```csharp
// 在你的自定義節點 ViewModel 中，例如一個處理雙精度浮點數的節點
public MyDoubleNodeViewModel()
{
    // 創建一個接受 double 型別的輸入端點
    var input = new ValueNodeInputViewModel<double>();
    
    // 覆寫 ConnectionValidator 以實現自定義邏輯
    input.ConnectionValidator = pending =>
    {
        // 檢查嘗試連接的輸出端是否為 double 型別的輸出
        if (pending.Output is ValueNodeOutputViewModel<double>)
            return new ConnectionValidationResult(true, null); // 允許連接
            
        // 或者，檢查嘗試連接的輸出端是否為 int 型別的輸出 (允許隱式轉型)
        if (pending.Output is ValueNodeOutputViewModel<int>)
            return new ConnectionValidationResult(true, null); // 也允許連接
            
        // 如果都不是以上兩種情況，則拒絕連接，並提供錯誤訊息
        return new ConnectionValidationResult(false, "型別不相容。此輸入端僅接受 Double 或 Integer 型別的輸出。");
    };
    
    // 將這個自定義的輸入端點加入到節點的輸入集合中
    this.Inputs.Add(input);
}
```

## 總結

每個節點與節點之間的連接關係，主要是由 **接收方 (即輸入端點 `NodeInputViewModel`)** 的 `ConnectionValidator` 函式來負責檢查和把關的。

*   **預設情況 (核心庫)**: 僅強制執行基礎的物理和拓撲限制。
*   **預設情況 (Toolkit)**: 在核心限制之上，進一步增加了嚴格的型別匹配檢查。
*   **客製化**: 開發者可以完全自定義 `ConnectionValidator` 的邏輯，以實現任何所需的連接規則。
