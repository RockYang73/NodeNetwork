namespace ActuatorApp.Core.Enums
{
    /// <summary>
    /// 節點類型
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// 電源供應器 (TP)
        /// </summary>
        PowerSupply,

        /// <summary>
        /// 電池 (TBB)
        /// </summary>
        Battery,

        /// <summary>
        /// 控制盒 (TC)
        /// </summary>
        ControlBox,

        /// <summary>
        /// 推桿 (TA)
        /// </summary>
        Actuator,

        /// <summary>
        /// 手控器 (TH/TFH/THP)
        /// </summary>
        Control,

        /// <summary>
        /// 配件 (TYC)
        /// </summary>
        Accessory,

        /// <summary>
        /// 參數節點
        /// </summary>
        Parameter
    }
}
