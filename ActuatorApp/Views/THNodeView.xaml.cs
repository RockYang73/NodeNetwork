using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActuatorApp.ViewModels;
using ActuatorApp.ViewModels.Nodes;
using ReactiveUI;

namespace ActuatorApp.Views
{
    public partial class THNodeView : UserControl, IViewFor<THNodeViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                typeof(THNodeViewModel), typeof(THNodeView),
                new PropertyMetadata(null));

        public THNodeViewModel ViewModel
        {
            get => (THNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (THNodeViewModel)value;
        }
        #endregion

        public THNodeView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                // 綁定 NodeView
                this.NodeView.ViewModel = this.ViewModel;
                Disposable.Create(() => this.NodeView.ViewModel = null).DisposeWith(d);

                // 設定節點背景顏色 (紫色表示 TH 控制節點)
                this.NodeView.Background = new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0));
            });
        }
    }
}
