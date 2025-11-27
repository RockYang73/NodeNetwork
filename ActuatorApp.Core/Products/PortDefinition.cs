using System.Collections.Generic;
using ActuatorApp.Core.Enums;

namespace ActuatorApp.Core.Products
{
    /// <summary>
    /// 端口定義
    /// </summary>
    public class PortDefinition
    {
        /// <summary>
        /// 端口名稱 (如 "M1", "H1", "Power", "TC")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 端口類型
        /// </summary>
        public PortType PortType { get; set; }

        /// <summary>
        /// 是否為動態數量端口 (如 TC 的 M1~Mn)
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// 端口數量 (當 IsDynamic 為 true 時使用)
        /// </summary>
        public int Count { get; set; } = 1;

        /// <summary>
        /// 端口說明
        /// </summary>
        public string Description { get; set; }
    }
}
