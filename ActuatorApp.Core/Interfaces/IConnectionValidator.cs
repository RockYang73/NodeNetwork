namespace ActuatorApp.Core.Interfaces
{
    /// <summary>
    /// 連線驗證器介面 - 定義連線驗證的核心邏輯，與 UI 框架無關
    /// </summary>
    public interface IConnectionValidator
    {
        /// <summary>
        /// 驗證連線是否有效
        /// </summary>
        /// <param name="sourceNode">來源節點</param>
        /// <param name="sourcePort">來源端口</param>
        /// <param name="targetNode">目標節點</param>
        /// <param name="targetPort">目標端口</param>
        /// <returns>驗證結果</returns>
        ConnectionValidationResult Validate(
            IActuatorNode sourceNode,
            IActuatorPort sourcePort,
            IActuatorNode targetNode,
            IActuatorPort targetPort);
    }

    /// <summary>
    /// 連線驗證結果
    /// </summary>
    public class ConnectionValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// 錯誤訊息 (當 IsValid 為 false 時)
        /// </summary>
        public string ErrorMessage { get; }

        public ConnectionValidationResult(bool isValid, string errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// 建立成功結果
        /// </summary>
        public static ConnectionValidationResult Success() => new ConnectionValidationResult(true);

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        public static ConnectionValidationResult Failure(string message) => new ConnectionValidationResult(false, message);
    }
}
