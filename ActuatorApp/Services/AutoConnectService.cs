using System.Linq;
using ActuatorApp.Validation;
using ActuatorApp.ViewModels;
using DynamicData; // 為了使用 Edit 和 Add
using NodeNetwork.ViewModels;

namespace ActuatorApp.Services
{
    public class AutoConnectService : IAutoConnectService
    {
        public void TryAutoConnect(NetworkViewModel network, NodeViewModel newNode)
        {
            // 取得網路上除了新節點以外的所有節點
            var existingNodes = network.Nodes.Items.Where(n => n != newNode).ToList();

            // 遍歷新節點的所有 Input
            foreach (var input in newNode.Inputs.Items)
            {
                // 檢查端口類型
                var inputPort = input.Port as ActuatorPortViewModel;
                if (inputPort == null) continue;

                // 檢查是否已經連接
                if (input.Connections.Count > 0) continue;

                // 在現有節點中尋找兼容的輸出
                foreach (var existingNode in existingNodes)
                {
                    foreach (var output in existingNode.Outputs.Items)
                    {
                        var outputPort = output.Port as ActuatorPortViewModel;
                        if (outputPort == null) continue;

                        // 檢查 PortType 是否相同
                        if (inputPort.PortType != outputPort.PortType) continue;

                        // 建立臨時的 PendingConnection 進行驗證
                        var pending = new PendingConnectionViewModel(network)
                        {
                            Input = input,
                            Output = output
                        };

                        // 呼叫驗證邏輯
                        var validationResult = ConnectionRules.ValidateConnection(pending, inputPort.PortType);

                        if (validationResult.IsValid)
                        {
                            // 建立連線
                            var connection = network.ConnectionFactory(input, output);
                            network.Connections.Edit(l => l.Add(connection));

                            // 找到第一個匹配的就停止，避免多重連接到同一個 Input
                            goto NextInput;
                        }
                    }
                }
            NextInput:;
            }
        }
    }
}
