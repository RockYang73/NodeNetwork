using ActuatorApp.Validation;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ActuatorApp.ViewModels
{
    /// <summary>
    /// 自定義 Input 端口
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    public class ActuatorInputViewModel<T> : ValueNodeInputViewModel<T>
    {
        static ActuatorInputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new NodeInputView(),
                typeof(IViewFor<ActuatorInputViewModel<T>>));
        }

        /// <summary>
        /// 端口類型
        /// </summary>
        public PortType PortType { get; }

        public ActuatorInputViewModel(PortType type)
        {
            PortType = type;
            Port = new ActuatorPortViewModel { PortType = type };

            // 設定連線驗證器
            ConnectionValidator = pending => ConnectionRules.ValidateConnection(pending, type);
        }
    }
}
