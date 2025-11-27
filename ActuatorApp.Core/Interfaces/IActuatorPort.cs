using ActuatorApp.Core.Enums;

namespace ActuatorApp.Core.Interfaces
{
    /// <summary>
    /// 端口介面 - 定義端口的核心屬性，與 UI 框架無關
    /// </summary>
    public interface IActuatorPort
    {
        /// <summary>
        /// 端口類型
        /// </summary>
        PortType PortType { get; }

        /// <summary>
        /// 端口名稱
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 所屬節點
        /// </summary>
        IActuatorNode ParentNode { get; }
    }
}
