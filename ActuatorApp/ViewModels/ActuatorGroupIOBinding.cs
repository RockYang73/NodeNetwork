using System; // Added for Action<T>
using System.Collections.Generic;
using DynamicData;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;
using ActuatorApp.ViewModels;
using System.Reactive.Linq; // Added

namespace ActuatorApp.ViewModels
{
    public class ActuatorGroupIOBinding : ValueNodeGroupIOBinding
    {
        public ActuatorGroupIOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode) 
            : base(groupNode, entranceNode, exitNode)
        {
        }

        #region Endpoint Create
        public override ValueNodeOutputViewModel<T> CreateCompatibleOutput<T>(ValueNodeInputViewModel<T> input)
        {
            var actuatorInput = input as ActuatorInputViewModel<T>;
            var portType = actuatorInput?.PortType ?? ActuatorApp.ViewModels.PortType.Power; // Default fallback

            return new ActuatorOutputViewModel<T>(portType)
            {
                Name = input.Name,
                Icon = input.Icon
            };
        }

        public override ValueNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<T> output)
        {
            var actuatorOutput = output as ActuatorOutputViewModel<T>;
            var portType = actuatorOutput?.PortType ?? ActuatorApp.ViewModels.PortType.Power; // Default fallback

            return new ActuatorInputViewModel<T>(portType)
            {
                Name = output.Name,
                Icon = output.Icon
            };
        }
        #endregion

        #region Endpoint Add
        public override NodeInputViewModel AddNewGroupNodeInput(NodeOutputViewModel candidateOutput)
        {
            var input = base.AddNewGroupNodeInput(candidateOutput);
            input.Name = "In";

            // 自動移除邏輯：當連線建立後又斷開（數量歸零）時，移除此端口
            input.Connections.CountChanged
                .SkipWhile(count => count == 0) // 等待初始連線建立
                .Where(count => count == 0)     // 當連線斷開
                .Take(1)                        // 只觸發一次
                .Subscribe(_ => { DeleteEndpoint(input); });

            return input;
        }

        public override NodeOutputViewModel AddNewGroupNodeOutput(NodeInputViewModel candidateInput)
        {
            var output = base.AddNewGroupNodeOutput(candidateInput);
            output.Name = "Out";

            // 自動移除邏輯：當連線建立後又斷開（數量歸零）時，移除此端口
            output.Connections.CountChanged
                .SkipWhile(count => count == 0) // 等待初始連線建立
                .Where(count => count == 0)     // 當連線斷開
                .Take(1)                        // 只觸發一次
                .Subscribe(_ => { DeleteEndpoint(output); });

            return output;
        }
        #endregion
    }
}
