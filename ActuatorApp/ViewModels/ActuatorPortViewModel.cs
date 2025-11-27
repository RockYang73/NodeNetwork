using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ActuatorApp.ViewModels
{
    /// <summary>
    /// 端口資料類型
    /// </summary>
    public enum PortType
    {
        /// <summary>
        /// 電流 (藍色)
        /// </summary>
        Power,

        /// <summary>
        /// 二進制控制信號 (灰色)
        /// </summary>
        BinaryData,

        /// <summary>
        /// 參數 (黃色)
        /// </summary>
        Parameter
    }

    /// <summary>
    /// 自定義端口 ViewModel
    /// </summary>
    public class ActuatorPortViewModel : PortViewModel
    {
        static ActuatorPortViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new Views.ActuatorPortView(),
                typeof(IViewFor<ActuatorPortViewModel>));
        }

        #region PortType
        private PortType _portType;
        /// <summary>
        /// 端口類型
        /// </summary>
        public PortType PortType
        {
            get => _portType;
            set => this.RaiseAndSetIfChanged(ref _portType, value);
        }
        #endregion
    }
}
