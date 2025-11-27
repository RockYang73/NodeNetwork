using ActuatorApp.Models;
using ActuatorApp.Views;
using DynamicData;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// THP 系列 - Control 節點 (手持控制器)
    /// Input: TC (控制信號)
    /// </summary>
    public class THPNodeViewModel : ActuatorNodeViewModel
    {
        static THPNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new ActuatorNodeView(),
                typeof(IViewFor<THPNodeViewModel>));
        }

        /// <summary>
        /// 控制信號輸入端口
        /// </summary>
        public ActuatorInputViewModel<BinarySignal> TCIn { get; }

        public THPNodeViewModel(string model = "THP") : base(NodeType.Control)
        {
            Name = model;
            ModelNumber = model;

            // 控制信號輸入
            TCIn = new ActuatorInputViewModel<BinarySignal>(PortType.BinaryData)
            {
                Name = "TC"
            };
            Inputs.Add(TCIn);
        }
    }
}
