using System.Collections.Generic;
using System.Reactive.Linq;
using ActuatorApp.Models;
using ActuatorApp.Views;
using DynamicData;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// TC 系列 - Control Box 節點
    /// Input: Power (電流), H1 (手控器信號)
    /// Output: M1~Mn (推桿控制, 動態數量)
    /// </summary>
    public class TCNodeViewModel : ActuatorNodeViewModel
    {
        static TCNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new ActuatorNodeView(),
                typeof(IViewFor<TCNodeViewModel>));
        }

        /// <summary>
        /// 電流輸入端口
        /// </summary>
        public ActuatorInputViewModel<PowerSignal> PowerIn { get; }

        /// <summary>
        /// 手控器信號輸入端口
        /// </summary>
        public ActuatorInputViewModel<BinarySignal> H1In { get; }

        /// <summary>
        /// 推桿控制輸出端口列表 (M1~Mn)
        /// </summary>
        public List<ActuatorOutputViewModel<BinarySignal>> MotorOutputs { get; } = new List<ActuatorOutputViewModel<BinarySignal>>();

        /// <summary>
        /// 推桿端口數量
        /// </summary>
        public int MotorCount { get; }

        public TCNodeViewModel(string model = "TC14", int motorCount = 3) : base(NodeType.ControlBox)
        {
            Name = model;
            ModelNumber = model;
            MotorCount = motorCount;

            // 電流輸入
            PowerIn = new ActuatorInputViewModel<PowerSignal>(PortType.Power)
            {
                Name = "Power"
            };
            Inputs.Add(PowerIn);

            // 動態建立 M1~Mn 輸出端口
            for (int i = 1; i <= motorCount; i++)
            {
                int channelIndex = i; // 捕獲變數
                var motorOut = new ActuatorOutputViewModel<BinarySignal>(PortType.BinaryData)
                {
                    Name = $"M{i}",
                    Value = Observable.Return(new BinarySignal
                    {
                        Channel = $"M{channelIndex}",
                        SourceNode = model
                    })
                };
                MotorOutputs.Add(motorOut);
                Outputs.Add(motorOut);
            }

            // 手控器信號輸入 (放在最後)
            H1In = new ActuatorInputViewModel<BinarySignal>(PortType.BinaryData)
            {
                Name = "H1"
            };
            Inputs.Add(H1In);
        }
    }
}
