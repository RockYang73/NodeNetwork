using System.Reactive.Linq;
using ActuatorApp.Models;
using ActuatorApp.Views;
using DynamicData;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// TBB 系列 - Battery 節點
    /// Input: Power (來自 TP)
    /// Output: Power (至 TC)
    /// </summary>
    public class TBBNodeViewModel : ActuatorNodeViewModel
    {
        static TBBNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new ActuatorNodeView(),
                typeof(IViewFor<TBBNodeViewModel>));
        }

        /// <summary>
        /// 電流輸入端口 (來自 TP)
        /// </summary>
        public ActuatorInputViewModel<PowerSignal> PowerIn { get; }

        /// <summary>
        /// 電流輸出端口 (至 TC)
        /// </summary>
        public ActuatorOutputViewModel<PowerSignal> PowerOut { get; }

        public TBBNodeViewModel(string model = "TBB3") : base(NodeType.Battery)
        {
            Name = model;
            ModelNumber = model;

            // 電流輸入端口
            PowerIn = new ActuatorInputViewModel<PowerSignal>(PortType.Power)
            {
                Name = "In"
            };
            Inputs.Add(PowerIn);

            // 電流輸出端口 (透過電池轉發)
            PowerOut = new ActuatorOutputViewModel<PowerSignal>(PortType.Power)
            {
                Name = "Out",
                Value = PowerIn.ValueChanged.Select(p => p ?? new PowerSignal
                {
                    SourceNode = model,
                    IsActive = false
                })
            };
            Outputs.Add(PowerOut);
        }
    }
}
