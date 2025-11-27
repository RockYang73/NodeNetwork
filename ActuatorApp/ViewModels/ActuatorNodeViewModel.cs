using System.Windows.Media;
using ActuatorApp.Core.Interfaces;
using NodeNetwork.ViewModels;
using ReactiveUI;
using CoreNodeType = ActuatorApp.Core.Enums.NodeType;

namespace ActuatorApp.ViewModels
{
    /// <summary>
    /// 節點類型 (對應產品分類)
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// 電源供應器 (TP)
        /// </summary>
        PowerSupply,

        /// <summary>
        /// 電池 (TBB)
        /// </summary>
        Battery,

        /// <summary>
        /// 控制盒 (TC)
        /// </summary>
        ControlBox,

        /// <summary>
        /// 推桿 (TA)
        /// </summary>
        Actuator,

        /// <summary>
        /// 手控器 (TH/TFH)
        /// </summary>
        Control,

        /// <summary>
        /// 配件 (TYC/THP)
        /// </summary>
        Accessory,

        /// <summary>
        /// 參數節點 (黃色橢圓)
        /// </summary>
        Parameter
    }

    /// <summary>
    /// 通訊協定類型 (TH/TFH 單選)
    /// </summary>
    public enum ProtocolType
    {
        GPIO,
        Scan,
        TBUS
    }

    /// <summary>
    /// 電動推桿系統節點基類
    /// </summary>
    public class ActuatorNodeViewModel : NodeViewModel, IActuatorNode
    {
        static ActuatorNodeViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new Views.ActuatorNodeView(),
                typeof(IViewFor<ActuatorNodeViewModel>));
        }

        /// <summary>
        /// 節點類型
        /// </summary>
        public NodeType NodeType { get; }

        /// <summary>
        /// 實作 IActuatorNode - 取得核心層 NodeType
        /// </summary>
        CoreNodeType IActuatorNode.NodeType => MapToCore(NodeType);

        /// <summary>
        /// 實作 IActuatorNode - 取得顯示名稱
        /// </summary>
        string IActuatorNode.DisplayName => Name;

        #region ProductImage
        private ImageSource _productImage;
        /// <summary>
        /// 產品圖片
        /// </summary>
        public ImageSource ProductImage
        {
            get => _productImage;
            set => this.RaiseAndSetIfChanged(ref _productImage, value);
        }
        #endregion

        #region ModelNumber
        private string _modelNumber;
        /// <summary>
        /// 產品型號
        /// </summary>
        public string ModelNumber
        {
            get => _modelNumber;
            set => this.RaiseAndSetIfChanged(ref _modelNumber, value);
        }
        #endregion

        public ActuatorNodeViewModel(NodeType type)
        {
            NodeType = type;
        }

        /// <summary>
        /// 將 ActuatorApp.NodeType 映射到 Core.NodeType
        /// </summary>
        private static CoreNodeType MapToCore(NodeType type)
        {
            return type switch
            {
                NodeType.PowerSupply => CoreNodeType.PowerSupply,
                NodeType.Battery => CoreNodeType.Battery,
                NodeType.ControlBox => CoreNodeType.ControlBox,
                NodeType.Actuator => CoreNodeType.Actuator,
                NodeType.Control => CoreNodeType.Control,
                NodeType.Accessory => CoreNodeType.Accessory,
                NodeType.Parameter => CoreNodeType.Parameter,
                _ => CoreNodeType.Accessory
            };
        }
    }
}
