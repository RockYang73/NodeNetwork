using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActuatorApp.ViewModels;
using ReactiveUI;

namespace ActuatorApp.Views
{
    public partial class ActuatorNodeView : UserControl, IViewFor<ActuatorNodeViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                typeof(ActuatorNodeViewModel), typeof(ActuatorNodeView),
                new PropertyMetadata(null));

        public ActuatorNodeViewModel ViewModel
        {
            get => (ActuatorNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ActuatorNodeViewModel)value;
        }
        
        // 為 XAML 綁定提供 ProductImage 屬性
        public static readonly DependencyProperty ProductImageSourceProperty =
            DependencyProperty.Register(nameof(ProductImageSource),
                typeof(ImageSource), typeof(ActuatorNodeView),
                new PropertyMetadata(null));

        public ImageSource ProductImageSource
        {
            get => (ImageSource)GetValue(ProductImageSourceProperty);
            set => SetValue(ProductImageSourceProperty, value);
        }
        #endregion

        public ActuatorNodeView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                System.Diagnostics.Debug.WriteLine($"[ActuatorNodeView] WhenActivated - ViewModel: {ViewModel?.Name}, ProductImage is null? {ViewModel?.ProductImage == null}");
                
                NodeView.ViewModel = this.ViewModel;
                Disposable.Create(() => NodeView.ViewModel = null).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.NodeType, v => v.NodeView.Background,
                    ConvertNodeTypeToBrush).DisposeWith(d);

                // 同時更新 DependencyProperty 供 XAML Visibility 綁定使用
                this.OneWayBind(ViewModel, vm => vm.ProductImage, v => v.ProductImageSource)
                    .DisposeWith(d);
            });
        }

        /// <summary>
        /// 根據節點類型返回對應的背景色
        /// </summary>
        public static Brush ConvertNodeTypeToBrush(NodeType type)
        {
            switch (type)
            {
                case NodeType.PowerSupply:
                    return new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // 綠色
                case NodeType.Battery:
                    return new SolidColorBrush(Color.FromRgb(0x8B, 0xC3, 0x4A)); // 淺綠
                case NodeType.ControlBox:
                    return new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)); // 藍色
                case NodeType.Actuator:
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // 橙色
                case NodeType.Control:
                    return new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)); // 紫色
                case NodeType.Accessory:
                    return new SolidColorBrush(Color.FromRgb(0x60, 0x7D, 0x8B)); // 灰色
                case NodeType.Parameter:
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)); // 黃色
                default:
                    return new SolidColorBrush(Color.FromRgb(0x49, 0x49, 0x49)); // 深灰
            }
        }
    }
}
