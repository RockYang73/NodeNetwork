namespace ActuatorApp.Models
{
    /// <summary>
    /// 參數值 (黃色橢圓節點)
    /// </summary>
    public class ParameterValue
    {
        /// <summary>
        /// 參數名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 參數值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 值類型 ("int", "string" 等)
        /// </summary>
        public string ValueType { get; set; }

        /// <summary>
        /// 取得字串表示
        /// </summary>
        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
