using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActuatorApp.ViewModels.Nodes;
using ReactiveUI;

namespace ActuatorApp.Views
{
    /// <summary>
    /// GroupNodeView - 使用 NodeView 作為基礎的容器樣式群組節點視圖
    /// 保留完整的拖曳、選取功能，並在內容區域顯示子節點網格
    /// </summary>
    public partial class GroupNodeView : UserControl, IViewFor<ContainerGroupNodeViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                typeof(ContainerGroupNodeViewModel), typeof(GroupNodeView),
                new PropertyMetadata(null));

        public ContainerGroupNodeViewModel ViewModel
        {
            get => (ContainerGroupNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ContainerGroupNodeViewModel)value;
        }
        #endregion

        public GroupNodeView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                if (ViewModel == null) return;

                System.Diagnostics.Debug.WriteLine($"[GroupNodeView] WhenActivated - ViewModel: {ViewModel?.Name}, Previews: {ViewModel?.SubnetNodePreviewsBinding?.Count}");

                // 將 ViewModel 綁定到內部的 NodeView
                NodeView.ViewModel = this.ViewModel;
                Disposable.Create(() => NodeView.ViewModel = null).DisposeWith(d);

                // 設定 NodeView 的背景顏色 (Header 顏色)
                this.WhenAnyValue(v => v.ViewModel.HeaderColor)
                    .Where(c => c != null)
                    .Subscribe(color =>
                    {
                        NodeView.Background = new SolidColorBrush(color.Value);
                    })
                    .DisposeWith(d);

                // 設定邊框顏色
                this.WhenAnyValue(v => v.ViewModel.BorderColor)
                    .Where(c => c != null)
                    .Subscribe(color =>
                    {
                        NodeView.BorderBrush = new SolidColorBrush(color.Value);
                    })
                    .DisposeWith(d);

                // Debug: 監聽預覽列表變化
                this.WhenAnyValue(v => v.ViewModel.SubnetNodePreviewsBinding.Count)
                    .Subscribe(count =>
                    {
                        System.Diagnostics.Debug.WriteLine($"[GroupNodeView] SubnetNodePreviewsBinding count: {count}");
                    })
                    .DisposeWith(d);
            });
        }
    }
}
