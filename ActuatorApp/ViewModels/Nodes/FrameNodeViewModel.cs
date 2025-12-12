using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using DynamicData;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    /// <summary>
    /// FrameNodeViewModel - 視覺分組容器
    /// 類似 Blender Geometry Nodes 的 Frame，用於視覺組織節點
    /// 不改變節點結構，內部節點完整顯示
    /// </summary>
    public class FrameNodeViewModel : ReactiveObject
    {
        #region Name
        private string _name = "Frame";
        /// <summary>
        /// Frame 標題
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        #endregion

        #region Position
        private Point _position;
        /// <summary>
        /// Frame 左上角位置
        /// </summary>
        public Point Position
        {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }
        #endregion

        #region Size
        private Size _size = new Size(200, 150);
        /// <summary>
        /// Frame 尺寸
        /// </summary>
        public Size Size
        {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
        }
        #endregion

        #region BorderColor
        private Color _borderColor = Color.FromRgb(0x00, 0x7A, 0xCC); // 藍色
        /// <summary>
        /// 邊框顏色
        /// </summary>
        public Color BorderColor
        {
            get => _borderColor;
            set => this.RaiseAndSetIfChanged(ref _borderColor, value);
        }
        #endregion

        #region BackgroundColor
        private Color _backgroundColor = Color.FromArgb(0x30, 0x00, 0x7A, 0xCC); // 半透明藍色
        /// <summary>
        /// 背景顏色 (半透明)
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => this.RaiseAndSetIfChanged(ref _backgroundColor, value);
        }
        #endregion

        #region IsSelected
        private bool _isSelected;
        /// <summary>
        /// 是否被選取
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
        #endregion

        #region HeaderHeight
        /// <summary>
        /// Header 高度 (常數)
        /// </summary>
        public double HeaderHeight => 28;
        #endregion

        #region Padding
        /// <summary>
        /// 內部節點與邊框的間距
        /// </summary>
        public double Padding => 15;
        #endregion

        /// <summary>
        /// 包含的節點列表
        /// </summary>
        private readonly SourceList<NodeViewModel> _containedNodes = new SourceList<NodeViewModel>();
        public IObservableList<NodeViewModel> ContainedNodes => _containedNodes.AsObservableList();

        /// <summary>
        /// 父 Network (用於訪問節點)
        /// </summary>
        public NetworkViewModel ParentNetwork { get; set; }

        private CompositeDisposable _subscriptions = new CompositeDisposable();

        public FrameNodeViewModel()
        {
        }

        /// <summary>
        /// 添加節點到 Frame
        /// </summary>
        public void AddNode(NodeViewModel node)
        {
            if (node == null || _containedNodes.Items.Contains(node)) return;
            
            _containedNodes.Add(node);
            
            // 監聽節點位置變化，自動調整 Frame 邊界
            node.WhenAnyValue(n => n.Position)
                .Subscribe(_ => UpdateBoundsFromNodes())
                .DisposeWith(_subscriptions);
            
            UpdateBoundsFromNodes();
        }

        /// <summary>
        /// 從 Frame 移除節點
        /// </summary>
        public void RemoveNode(NodeViewModel node)
        {
            if (node == null) return;
            _containedNodes.Remove(node);
            UpdateBoundsFromNodes();
        }

        /// <summary>
        /// 添加多個節點
        /// </summary>
        public void AddNodes(IEnumerable<NodeViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                if (!_containedNodes.Items.Contains(node))
                {
                    _containedNodes.Add(node);
                }
            }
            UpdateBoundsFromNodes();
        }

        /// <summary>
        /// 根據內部節點計算並更新邊界
        /// </summary>
        public void UpdateBoundsFromNodes()
        {
            if (_containedNodes.Count == 0) return;

            var nodes = _containedNodes.Items.ToList();
            
            // 計算所有節點的邊界
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (var node in nodes)
            {
                minX = Math.Min(minX, node.Position.X);
                minY = Math.Min(minY, node.Position.Y);
                
                // 假設節點最小尺寸為 150x100
                var nodeWidth = Math.Max(node.Size.Width, 150);
                var nodeHeight = Math.Max(node.Size.Height, 100);
                
                maxX = Math.Max(maxX, node.Position.X + nodeWidth);
                maxY = Math.Max(maxY, node.Position.Y + nodeHeight);
            }

            // 添加 padding 和 header 空間
            Position = new Point(minX - Padding, minY - HeaderHeight - Padding);
            Size = new Size(
                maxX - minX + Padding * 2,
                maxY - minY + HeaderHeight + Padding * 2
            );
        }

        /// <summary>
        /// 移動 Frame（連同內部節點一起移動）
        /// </summary>
        public void Move(Vector delta)
        {
            // 更新 Frame 位置
            Position = new Point(Position.X + delta.X, Position.Y + delta.Y);
            
            // 更新所有內部節點位置
            foreach (var node in _containedNodes.Items)
            {
                node.Position = new Point(
                    node.Position.X + delta.X,
                    node.Position.Y + delta.Y
                );
            }
        }

        /// <summary>
        /// 設定 Frame 顏色主題
        /// </summary>
        public void SetTheme(Color borderColor)
        {
            BorderColor = borderColor;
            BackgroundColor = Color.FromArgb(0x30, borderColor.R, borderColor.G, borderColor.B);
        }

        /// <summary>
        /// 清理訂閱
        /// </summary>
        public void Dispose()
        {
            _subscriptions?.Dispose();
            _containedNodes.Clear();
        }
    }
}
