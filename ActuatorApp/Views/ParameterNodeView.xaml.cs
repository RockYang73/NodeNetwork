using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using ActuatorApp.ViewModels.Nodes;
using ReactiveUI;

namespace ActuatorApp.Views
{
    public partial class ParameterNodeView : UserControl, IViewFor<ParameterNodeViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                typeof(ParameterNodeViewModel), typeof(ParameterNodeView),
                new PropertyMetadata(null));

        public ParameterNodeViewModel ViewModel
        {
            get => (ParameterNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ParameterNodeViewModel)value;
        }
        #endregion

        public ParameterNodeView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                // 綁定參數名稱
                this.OneWayBind(ViewModel, vm => vm.ParameterName, v => v.ParamName.Text)
                    .DisposeWith(d);

                // 綁定編輯器值
                this.Bind(ViewModel, vm => vm.Editor.Value, v => v.ValueEditor.Text)
                    .DisposeWith(d);
            });
        }
    }
}
