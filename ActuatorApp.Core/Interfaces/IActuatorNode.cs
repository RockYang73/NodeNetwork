using ActuatorApp.Core.Enums;

namespace ActuatorApp.Core.Interfaces
{
    /// <summary>
    /// 節點介面 - 定義節點的核心屬性，與 UI 框架無關
    /// </summary>
    public interface IActuatorNode
    {
        /// <summary>
        /// 節點類型
        /// </summary>
        NodeType NodeType { get; }

        /// <summary>
        /// 產品型號
        /// </summary>
        string ModelNumber { get; }

        /// <summary>
        /// 節點顯示名稱
        /// </summary>
        string DisplayName { get; }
    }
}
