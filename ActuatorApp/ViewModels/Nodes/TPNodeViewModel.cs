using System.Reactive.Linq;
using ActuatorApp.Models;
using ActuatorApp.Views;
using DynamicData;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// TP 系列 - Power Supply 節點
    /// Output: Power (電流輸出)
    /// </summary>
    public class TPNodeViewModel : ActuatorNodeViewModel
    {
        static TPNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new ActuatorNodeView(),
                typeof(IViewFor<TPNodeViewModel>));
        }

        /// <summary>
        /// 電流輸出端口
        /// </summary>
        public ActuatorOutputViewModel<PowerSignal> PowerOut { get; }

        public TPNodeViewModel(string model = "TP5") : base(NodeType.PowerSupply)
        {
            Name = model;
            ModelNumber = model;

            // 電流輸出端口
            PowerOut = new ActuatorOutputViewModel<PowerSignal>(PortType.Power)
            {
                Name = "Out",
                Value = Observable.Return(new PowerSignal
                {
                    Voltage = 24.0,
                    SourceNode = model,
                    IsActive = true
                })
            };
            Outputs.Add(PowerOut);
        }
    }
}
