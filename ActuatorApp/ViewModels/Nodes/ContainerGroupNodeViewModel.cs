using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using DynamicData;
using NodeNetwork;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// 子節點預覽資訊
    /// </summary>
    public class SubnetNodePreview : ReactiveObject
    {
        public string Name { get; set; }
        public ImageSource Image { get; set; }
        public NodeType NodeType { get; set; }
        public string ModelNumber { get; set; }
    }

    /// <summary>
    /// ContainerGroupNodeViewModel - 帶有 Header 容器樣式的群組節點
    /// 使用 NodeView 作為基礎以保留拖曳和選取功能
    /// </summary>
    public class ContainerGroupNodeViewModel : NodeViewModel
    {
        static ContainerGroupNodeViewModel()
        {
            // 註冊使用新的 GroupNodeView
            NNViewRegistrar.AddRegistration(() => new ActuatorApp.Views.GroupNodeView(), typeof(IViewFor<ContainerGroupNodeViewModel>));
        }

        /// <summary>
        /// 此群組節點所包含的子網路
        /// </summary>
        public NetworkViewModel Subnet { get; }

        /// <summary>
        /// I/O 綁定，用於處理代理端口
        /// </summary>
        public NodeGroupIOBinding IOBinding { get; set; }

        #region SubnetNodePreviews
        private readonly SourceList<SubnetNodePreview> _subnetNodePreviews = new SourceList<SubnetNodePreview>();
        
        /// <summary>
        /// 子節點預覽列表 (用於在容器中顯示)
        /// </summary>
        public IObservableList<SubnetNodePreview> SubnetNodePreviews => _subnetNodePreviews.AsObservableList();
        
        /// <summary>
        /// 子節點預覽的只讀集合 (用於 XAML 綁定)
        /// </summary>
        private ReadOnlyObservableCollection<SubnetNodePreview> _subnetNodePreviewsBinding;
        public ReadOnlyObservableCollection<SubnetNodePreview> SubnetNodePreviewsBinding => _subnetNodePreviewsBinding;
        #endregion

        #region HeaderColor
        private Color? _headerColor;
        /// <summary>
        /// Header 區域的背景顏色
        /// </summary>
        public Color? HeaderColor
        {
            get => _headerColor;
            set => this.RaiseAndSetIfChanged(ref _headerColor, value);
        }
        #endregion

        #region BorderColor
        private Color? _borderColor;
        /// <summary>
        /// 容器邊框顏色
        /// </summary>
        public Color? BorderColor
        {
            get => _borderColor;
            set => this.RaiseAndSetIfChanged(ref _borderColor, value);
        }
        #endregion

        #region IsExpanded
        private bool _isExpanded = true;
        /// <summary>
        /// 內容區域是否展開
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }
        #endregion

        #region SubnetNodeCount
        private int _subnetNodeCount;
        /// <summary>
        /// 子網節點數量
        /// </summary>
        public int SubnetNodeCount
        {
            get => _subnetNodeCount;
            set => this.RaiseAndSetIfChanged(ref _subnetNodeCount, value);
        }
        #endregion

        public ContainerGroupNodeViewModel(NetworkViewModel subnet)
        {
            this.Subnet = subnet;
            this.Name = "Container Group";
            
            // 預設顏色
            this.HeaderColor = Color.FromRgb(0x2D, 0x2D, 0x30);  // 深灰色
            this.BorderColor = Color.FromRgb(0xFF, 0x6B, 0x6B);  // 紅色邊框

            // 綁定子節點預覽列表
            _subnetNodePreviews.Connect()
                .Bind(out _subnetNodePreviewsBinding)
                .Subscribe();

            // 監聽子網節點數量變化
            if (subnet != null)
            {
                subnet.Nodes.CountChanged
                    .Subscribe(count => SubnetNodeCount = count);
            }
        }

        /// <summary>
        /// 從被群組化的節點收集預覽資訊
        /// </summary>
        public void CollectNodePreviews(System.Collections.Generic.IEnumerable<NodeViewModel> nodes)
        {
            _subnetNodePreviews.Clear();
            
            foreach (var node in nodes)
            {
                if (node is ActuatorNodeViewModel actuatorNode)
                {
                    _subnetNodePreviews.Add(new SubnetNodePreview
                    {
                        Name = actuatorNode.Name,
                        Image = actuatorNode.ProductImage,
                        NodeType = actuatorNode.NodeType,
                        ModelNumber = actuatorNode.ModelNumber
                    });
                }
                else if (node is ParameterNodeViewModel paramNode)
                {
                    _subnetNodePreviews.Add(new SubnetNodePreview
                    {
                        Name = paramNode.Name,
                        Image = null,
                        NodeType = NodeType.Parameter,
                        ModelNumber = "Parameter"
                    });
                }
                else
                {
                    // 其他類型節點
                    _subnetNodePreviews.Add(new SubnetNodePreview
                    {
                        Name = node.Name ?? "Node",
                        Image = null,
                        NodeType = NodeType.Accessory,
                        ModelNumber = "Unknown"
                    });
                }
            }
        }

        /// <summary>
        /// 設定群組的主題顏色
        /// </summary>
        public void SetThemeColors(Color headerColor, Color borderColor)
        {
            HeaderColor = headerColor;
            BorderColor = borderColor;
        }
    }
}
