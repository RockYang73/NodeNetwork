using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActuatorApp.ViewModels;
using ActuatorApp.ViewModels.Nodes;
using ReactiveUI;

namespace ActuatorApp.Views
{
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                typeof(MainViewModel), typeof(MainWindow),
                new PropertyMetadata(null, (d, e) => ((MainWindow)d).DataContext = e.NewValue));

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                // 綁定網路視圖
                this.OneWayBind(ViewModel, vm => vm.Network, v => v.NetworkView.ViewModel)
                    .DisposeWith(d);

                // 綁定 Frames 視覺分組
                this.OneWayBind(ViewModel, vm => vm.FramesBound, v => v.FramesControl.ItemsSource)
                    .DisposeWith(d);

                // 注入 Frames 到 NetworkView 內部
                InjectFramesIntoNetworkView();

                // 綁定節點列表
                BindNodeLists();

                // 新增參數節點按鈕
                AddParameterButton.Click += (s, e) => ViewModel.AddParameterNode("Param");
                
                // 點擊 NetworkView 時清除 Frame 選取 (使用 PreviewMouseLeftButtonDown 的低優先級)
                NetworkView.MouseLeftButtonDown += (s, e) =>
                {
                    ViewModel?.ClearFrameSelection();
                };
            });
        }

        /// <summary>
        /// 將 FramesControl 注入到 NetworkView 的 contentContainer 中
        /// 這樣 Frame 就會跟隨 NetworkView 的 pan/zoom
        /// </summary>
        private void InjectFramesIntoNetworkView()
        {
            // 找到 NetworkView 內部的 contentContainer Canvas
            var contentContainer = FindChild<Canvas>(NetworkView, "contentContainer");
            if (contentContainer == null)
            {
                System.Diagnostics.Debug.WriteLine("[MainWindow] contentContainer not found in NetworkView");
                return;
            }

            // 從原本的父容器移除 FramesControl
            if (FramesControl.Parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(FramesControl);
            }

            // 將 FramesControl 插入到 contentContainer 的最前面 (在 backgroundCanvas 之後)
            contentContainer.Children.Insert(1, FramesControl);
            
            // 設置 FramesControl 的尺寸以填滿整個區域
            FramesControl.Width = double.NaN; // Auto
            FramesControl.Height = double.NaN;
            
            System.Diagnostics.Debug.WriteLine("[MainWindow] FramesControl injected into contentContainer");
        }

        /// <summary>
        /// 遞迴查找視覺樹中的子元素
        /// </summary>
        private static T FindChild<T>(System.Windows.DependencyObject parent, string childName) where T : System.Windows.DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    if (child is FrameworkElement fe && fe.Name == childName)
                    {
                        return typedChild;
                    }
                }

                var foundChild = FindChild<T>(child, childName);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }

            return null;
        }

        /// <summary>
        /// 綁定節點列表到各 Tab
        /// </summary>
        private void BindNodeLists()
        {
            var categories = ViewModel.NodeCategories;

            // Control Box
            var controlBox = categories.FirstOrDefault(c => c.Name == "Control Box");
            if (controlBox != null)
            {
                ControlBoxNodes.ItemsSource = controlBox.Nodes;
                SetupNodeItemClick(ControlBoxNodes);
            }

            // Actuator
            var actuator = categories.FirstOrDefault(c => c.Name == "Actuator");
            if (actuator != null)
            {
                ActuatorNodes.ItemsSource = actuator.Nodes;
                SetupNodeItemClick(ActuatorNodes);
            }

            // Controls
            var controls = categories.FirstOrDefault(c => c.Name == "Controls");
            if (controls?.SubCategories != null)
            {
                var embedded = controls.SubCategories.FirstOrDefault(s => s.Name == "Embedded");
                if (embedded != null)
                {
                    EmbeddedNodes.ItemsSource = embedded.Nodes;
                    SetupNodeItemClick(EmbeddedNodes);
                }

                var wireHandset = controls.SubCategories.FirstOrDefault(s => s.Name == "Wire Handset");
                if (wireHandset != null)
                {
                    WireHandsetNodes.ItemsSource = wireHandset.Nodes;
                    SetupNodeItemClick(WireHandsetNodes);
                }

                var wirelessHandset = controls.SubCategories.FirstOrDefault(s => s.Name == "Wireless Handset");
                if (wirelessHandset != null)
                {
                    WirelessHandsetNodes.ItemsSource = wirelessHandset.Nodes;
                    SetupNodeItemClick(WirelessHandsetNodes);
                }

                var handheld = controls.SubCategories.FirstOrDefault(s => s.Name == "Handheld");
                if (handheld != null)
                {
                    HandheldNodes.ItemsSource = handheld.Nodes;
                    SetupNodeItemClick(HandheldNodes);
                }
            }

            // Accessories
            var accessories = categories.FirstOrDefault(c => c.Name == "Accessories");
            if (accessories != null)
            {
                AccessoryNodes.ItemsSource = accessories.Nodes;
                SetupNodeItemClick(AccessoryNodes);
            }

            // Power Supply
            var powerSupply = categories.FirstOrDefault(c => c.Name == "Power Supply");
            if (powerSupply != null)
            {
                PowerSupplyNodes.ItemsSource = powerSupply.Nodes;
                SetupNodeItemClick(PowerSupplyNodes);
            }

            // Battery
            var battery = categories.FirstOrDefault(c => c.Name == "Battery");
            if (battery != null)
            {
                BatteryNodes.ItemsSource = battery.Nodes;
                SetupNodeItemClick(BatteryNodes);
            }
        }

        /// <summary>
        /// 設定節點項目點擊事件
        /// </summary>
        private void SetupNodeItemClick(ItemsControl itemsControl)
        {
            itemsControl.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (e.OriginalSource is FrameworkElement element &&
                    element.DataContext is NodeItemViewModel nodeItem)
                {
                    // 建立新節點並加入網路
                    var newNode = nodeItem.Factory();
                    ViewModel.AddNode(newNode);
                }
            };
        }
    }
}
