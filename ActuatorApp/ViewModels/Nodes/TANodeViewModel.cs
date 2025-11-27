using ActuatorApp.Models;
using ActuatorApp.Views;
using DynamicData;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// TA 系列 - Actuator 節點
    /// Input: TC (控制信號), Code (參數), Stroke (參數)
    /// Output: a (保留)
    /// </summary>
    public class TANodeViewModel : ActuatorNodeViewModel
    {
        static TANodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new ActuatorNodeView(),
                typeof(IViewFor<TANodeViewModel>));
        }

        /// <summary>
        /// 控制信號輸入 (來自 TC 的 M 端口)
        /// </summary>
        public ActuatorInputViewModel<BinarySignal> TCIn { get; }

        /// <summary>
        /// 參數輸入: Code (推桿編碼)
        /// </summary>
        public ActuatorInputViewModel<ParameterValue> CodeIn { get; }

        /// <summary>
        /// 參數輸入: Stroke (行程設定)
        /// </summary>
        public ActuatorInputViewModel<ParameterValue> StrokeIn { get; }

        public TANodeViewModel(string model = "TA6") : base(NodeType.Actuator)
        {
            Name = model;
            ModelNumber = model;

            // 控制信號輸入 (來自 TC 的 M 端口)
            TCIn = new ActuatorInputViewModel<BinarySignal>(PortType.BinaryData)
            {
                Name = "TC"
            };
            Inputs.Add(TCIn);

            // 參數輸入: Code
            CodeIn = new ActuatorInputViewModel<ParameterValue>(PortType.Parameter)
            {
                Name = "Code"
            };
            Inputs.Add(CodeIn);

            // 參數輸入: Stroke
            StrokeIn = new ActuatorInputViewModel<ParameterValue>(PortType.Parameter)
            {
                Name = "Stroke"
            };
            Inputs.Add(StrokeIn);

            // a 端口 (保留, 暫不實作)
        }
    }
}
