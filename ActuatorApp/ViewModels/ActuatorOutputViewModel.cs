using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ActuatorApp.ViewModels
{
    /// <summary>
    /// 自定義 Output 端口
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    public class ActuatorOutputViewModel<T> : ValueNodeOutputViewModel<T>
    {
        static ActuatorOutputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new NodeOutputView(),
                typeof(IViewFor<ActuatorOutputViewModel<T>>));
        }

        /// <summary>
        /// 端口類型
        /// </summary>
        public PortType PortType { get; }

        public ActuatorOutputViewModel(PortType type)
        {
            PortType = type;
            Port = new ActuatorPortViewModel { PortType = type };
        }
    }
}
