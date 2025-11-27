namespace ActuatorApp.Models
{
    /// <summary>
    /// 電流信號 (TP → TBB → TC)
    /// </summary>
    public class PowerSignal
    {
        /// <summary>
        /// 電壓值
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// 來源節點
        /// </summary>
        public string SourceNode { get; set; }

        /// <summary>
        /// 電流狀態
        /// </summary>
        public bool IsActive { get; set; }
    }
}
