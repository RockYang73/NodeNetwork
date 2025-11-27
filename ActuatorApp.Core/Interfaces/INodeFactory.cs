using ActuatorApp.Core.Products;

namespace ActuatorApp.Core.Interfaces
{
    /// <summary>
    /// 節點工廠介面 - 根據產品定義建立節點，由各 UI 框架實作
    /// </summary>
    public interface INodeFactory
    {
        /// <summary>
        /// 根據產品定義建立節點
        /// </summary>
        /// <param name="product">產品定義</param>
        /// <returns>節點實例 (具體類型由實作決定)</returns>
        object CreateNode(ProductDefinition product);

        /// <summary>
        /// 根據產品型號建立節點
        /// </summary>
        /// <param name="modelNumber">產品型號</param>
        /// <returns>節點實例 (具體類型由實作決定)</returns>
        object CreateNodeByModel(string modelNumber);
    }
}
