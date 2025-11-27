using System.Reactive.Linq;
using ActuatorApp.Models;
using ActuatorApp.Views;
using DynamicData;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// TH/TFH 系列 - Control (手控器) 節點
    /// Output: TC (控制信號至 Control Box)
    /// 屬性: ProtocolType (GPIO/Scan/TBUS 單選)
    /// </summary>
    public class THNodeViewModel : ActuatorNodeViewModel
    {
        static THNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new THNodeView(),
                typeof(IViewFor<THNodeViewModel>));
        }

        /// <summary>
        /// 控制信號輸出 (至 TC 的 H1)
        /// </summary>
        public ActuatorOutputViewModel<BinarySignal> TCOut { get; }

        #region SelectedProtocol
        private ProtocolType _selectedProtocol = ProtocolType.GPIO;
        /// <summary>
        /// 選擇的通訊協定 (單選)
        /// </summary>
        public ProtocolType SelectedProtocol
        {
            get => _selectedProtocol;
            set => this.RaiseAndSetIfChanged(ref _selectedProtocol, value);
        }
        #endregion

        #region IsGpioSelected
        private bool _isGpioSelected = true;
        public bool IsGpioSelected
        {
            get => _isGpioSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isGpioSelected, value);
                if (value) SelectedProtocol = ProtocolType.GPIO;
            }
        }
        #endregion

        #region IsScanSelected
        private bool _isScanSelected;
        public bool IsScanSelected
        {
            get => _isScanSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isScanSelected, value);
                if (value) SelectedProtocol = ProtocolType.Scan;
            }
        }
        #endregion

        #region IsTbusSelected
        private bool _isTbusSelected;
        public bool IsTbusSelected
        {
            get => _isTbusSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isTbusSelected, value);
                if (value) SelectedProtocol = ProtocolType.TBUS;
            }
        }
        #endregion

        public THNodeViewModel(string model = "TFH6") : base(NodeType.Control)
        {
            Name = model;
            ModelNumber = model;

            // 控制信號輸出 (至 TC 的 H1)
            TCOut = new ActuatorOutputViewModel<BinarySignal>(PortType.BinaryData)
            {
                Name = "TC",
                Value = this.WhenAnyValue(x => x.SelectedProtocol)
                    .Select(protocol => new BinarySignal
                    {
                        Command = $"Protocol:{protocol}",
                        SourceNode = model
                    })
            };
            Outputs.Add(TCOut);

            // a 端口 (保留, 暫不實作)
        }

        /// <summary>
        /// Get ID 命令 (預留)
        /// </summary>
        public void GetId()
        {
            // TODO: 實作 Get ID 邏輯
        }
    }
}
