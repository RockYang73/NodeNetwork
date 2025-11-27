namespace ActuatorApp.Models
{
    /// <summary>
    /// 二進制控制信號 (TC ↔ TH, TC → TA)
    /// </summary>
    public class BinarySignal
    {
        /// <summary>
        /// 信號資料
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 控制命令 (Extend, Retract, Stop 等)
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 來源節點
        /// </summary>
        public string SourceNode { get; set; }

        /// <summary>
        /// 目標通道 (M1, M2, M3 等)
        /// </summary>
        public string Channel { get; set; }
    }
}
