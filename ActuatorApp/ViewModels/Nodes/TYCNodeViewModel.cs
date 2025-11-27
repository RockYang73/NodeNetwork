using System.Reactive.Linq;
using ActuatorApp.Models;
using ActuatorApp.Views;
using DynamicData;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// TYC 系列 - Accessory 節點 (延長線/分接線)
    /// 用途: Control 與 Control Box 之間的橋樑
    /// Input: IN1, IN2 (信號輸入 - 來自 Control 或其他 TYC)
    /// Output: Out (信號輸出 - 連接到 TC.H1 或其他 TYC)
    /// </summary>
    public class TYCNodeViewModel : ActuatorNodeViewModel
    {
        static TYCNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new ActuatorNodeView(),
                typeof(IViewFor<TYCNodeViewModel>));
        }

        /// <summary>
        /// 信號輸入端口 1 (來自 Control 或 TYC)
        /// </summary>
        public ActuatorInputViewModel<BinarySignal> IN1 { get; }

        /// <summary>
        /// 信號輸入端口 2 (來自 Control 或 TYC)
        /// </summary>
        public ActuatorInputViewModel<BinarySignal> IN2 { get; }

        /// <summary>
        /// 信號輸出端口 (連接到 TC.H1 或其他 TYC)
        /// </summary>
        public ActuatorOutputViewModel<BinarySignal> Out { get; }

        public TYCNodeViewModel(string model = "TYC") : base(NodeType.Accessory)
        {
            Name = model;
            ModelNumber = model;

            // 信號輸入 1
            IN1 = new ActuatorInputViewModel<BinarySignal>(PortType.BinaryData)
            {
                Name = "IN1"
            };
            Inputs.Add(IN1);

            // 信號輸入 2
            IN2 = new ActuatorInputViewModel<BinarySignal>(PortType.BinaryData)
            {
                Name = "IN2"
            };
            Inputs.Add(IN2);

            // 信號輸出 (合併兩個輸入的信號)
            Out = new ActuatorOutputViewModel<BinarySignal>(PortType.BinaryData)
            {
                Name = "Out",
                Value = IN1.ValueChanged
                    .CombineLatest(IN2.ValueChanged, (s1, s2) => 
                        s1 ?? s2 ?? new BinarySignal { SourceNode = model })
            };
            Outputs.Add(Out);
        }
    }
}
