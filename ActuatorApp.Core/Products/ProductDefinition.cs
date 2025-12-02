using System.Collections.Generic;
using ActuatorApp.Core.Enums;

namespace ActuatorApp.Core.Products
{
    /// <summary>
    /// 產品定義 - 描述一個產品的所有屬性
    /// </summary>
    public class ProductDefinition
    {
        /// <summary>
        /// 產品型號 (如 "TC14", "TH7", "TA6")
        /// </summary>
        public string ModelNumber { get; set; }

        /// <summary>
        /// 節點類型
        /// </summary>
        public NodeType NodeType { get; set; }

        /// <summary>
        /// 產品分類 (對應 Tab 名稱，如 "Control Box", "Actuator")
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 產品子分類 (如 "Embedded", "Wire Handset"，可為 null)
        /// </summary>
        public string SubCategory { get; set; }

        /// <summary>
        /// 產品顯示名稱 (預設使用 ModelNumber)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 產品說明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 輸入端口定義
        /// </summary>
        public List<PortDefinition> Inputs { get; set; } = new List<PortDefinition>();

        /// <summary>
        /// 輸出端口定義
        /// </summary>
        public List<PortDefinition> Outputs { get; set; } = new List<PortDefinition>();

        /// <summary>
        /// 產品特定屬性 (如 MotorCount, SupportedProtocols 等)
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 節點背景顏色 (Hex 格式，如 "#2196F3")
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// 產品圖片相對路徑 (如 "Controlbox/TC14.png")
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 取得顯示名稱 (如果 DisplayName 為空則使用 ModelNumber)
        /// </summary>
        public string GetDisplayName() => string.IsNullOrEmpty(DisplayName) ? ModelNumber : DisplayName;

        /// <summary>
        /// 取得指定屬性值
        /// </summary>
        public T GetProperty<T>(string key, T defaultValue = default)
        {
            if (Properties != null && Properties.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;
                
                // 嘗試轉換 (處理 JSON 反序列化的情況)
                try
                {
                    return (T)System.Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }
}
