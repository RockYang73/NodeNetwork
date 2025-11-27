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
                new PropertyMetadata(null));

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

            // 初始化 ViewModel
            ViewModel = new MainViewModel();

            this.WhenActivated(d =>
            {
                // 綁定網路視圖
                this.OneWayBind(ViewModel, vm => vm.Network, v => v.NetworkView.ViewModel)
                    .DisposeWith(d);

                // 綁定自動佈局按鈕
                this.BindCommand(ViewModel, vm => vm.AutoLayout, v => v.AutoLayoutButton)
                    .DisposeWith(d);

                // 綁定節點列表
                BindNodeLists();

                // 新增參數節點按鈕
                AddParameterButton.Click += (s, e) => ViewModel.AddParameterNode("Param");
            });
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
