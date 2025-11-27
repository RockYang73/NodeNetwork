using System.Reactive.Linq;
using ActuatorApp.Models;
using ActuatorApp.ViewModels.Editors;
using ActuatorApp.Views;
using DynamicData;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// Parameter 節點 (黃色橢圓)
    /// 支援連線與編輯器雙模式
    /// </summary>
    public class ParameterNodeViewModel : ActuatorNodeViewModel
    {
        static ParameterNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new ParameterNodeView(),
                typeof(IViewFor<ParameterNodeViewModel>));
        }

        /// <summary>
        /// 參數編輯器
        /// </summary>
        public ParameterValueEditorViewModel Editor { get; }

        /// <summary>
        /// 參數輸出端口
        /// </summary>
        public ActuatorOutputViewModel<ParameterValue> ValueOut { get; }

        /// <summary>
        /// 參數名稱
        /// </summary>
        public string ParameterName { get; }

        public ParameterNodeViewModel(string paramName = "C") : base(NodeType.Parameter)
        {
            Name = paramName;
            ParameterName = paramName;

            // 參數編輯器
            Editor = new ParameterValueEditorViewModel();

            // 參數輸出端口
            ValueOut = new ActuatorOutputViewModel<ParameterValue>(PortType.Parameter)
            {
                Name = "",
                Editor = Editor,
                Value = Editor.ValueChanged.Select(v => new ParameterValue
                {
                    Name = paramName,
                    Value = v,
                    ValueType = v?.GetType().Name ?? "string"
                })
            };
            Outputs.Add(ValueOut);
        }
    }
}
