using NodeNetwork.ViewModels;

namespace ActuatorApp.Services
{
    public interface IAutoConnectService
    {
        /// <summary>
        /// 嘗試將新節點自動連接到網絡中現有的兼容節點
        /// </summary>
        /// <param name="network">目標網絡</param>
        /// <param name="newNode">新加入的節點</param>
        void TryAutoConnect(NetworkViewModel network, NodeViewModel newNode);
    }
}
