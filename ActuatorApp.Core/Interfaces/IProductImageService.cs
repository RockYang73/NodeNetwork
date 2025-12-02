namespace ActuatorApp.Core.Interfaces
{
    /// <summary>
    /// 產品圖片服務介面 - 提供產品圖片路徑查詢功能
    /// </summary>
    public interface IProductImageService
    {
        /// <summary>
        /// 根據產品型號取得圖片完整路徑
        /// </summary>
        /// <param name="modelNumber">產品型號 (如 "TC14", "TH7")</param>
        /// <returns>圖片完整路徑，若找不到則回傳 null</returns>
        string GetImagePath(string modelNumber);

        /// <summary>
        /// 檢查指定型號的產品圖片是否存在
        /// </summary>
        /// <param name="modelNumber">產品型號</param>
        /// <returns>圖片存在則回傳 true</returns>
        bool ImageExists(string modelNumber);

        /// <summary>
        /// 取得預設圖片路徑 (當找不到對應圖片時使用)
        /// </summary>
        /// <returns>預設圖片路徑，若不使用預設圖片則回傳 null</returns>
        string GetDefaultImagePath();
    }
}
