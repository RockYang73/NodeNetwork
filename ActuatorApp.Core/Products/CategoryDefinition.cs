using System.Collections.Generic;

namespace ActuatorApp.Core.Products
{
    /// <summary>
    /// 產品分類定義
    /// </summary>
    public class CategoryDefinition
    {
        /// <summary>
        /// 分類名稱 (如 "Control Box", "Actuator", "Controls")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分類顯示順序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 分類說明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 子分類 (如 Controls 下的 "Embedded", "Wire Handset" 等)
        /// </summary>
        public List<SubCategoryDefinition> SubCategories { get; set; } = new List<SubCategoryDefinition>();

        /// <summary>
        /// 該分類下的產品 (當沒有子分類時直接放在這裡)
        /// </summary>
        public List<ProductDefinition> Products { get; set; } = new List<ProductDefinition>();
    }

    /// <summary>
    /// 產品子分類定義
    /// </summary>
    public class SubCategoryDefinition
    {
        /// <summary>
        /// 子分類名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 子分類顯示順序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 子分類說明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 該子分類下的產品
        /// </summary>
        public List<ProductDefinition> Products { get; set; } = new List<ProductDefinition>();
    }
}
