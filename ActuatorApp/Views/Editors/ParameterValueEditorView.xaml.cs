using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using ActuatorApp.ViewModels.Editors;
using ReactiveUI;

namespace ActuatorApp.Views.Editors
{
    public partial class ParameterValueEditorView : UserControl, IViewFor<ParameterValueEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                typeof(ParameterValueEditorViewModel), typeof(ParameterValueEditorView),
                new PropertyMetadata(null));

        public ParameterValueEditorViewModel ViewModel
        {
            get => (ParameterValueEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ParameterValueEditorViewModel)value;
        }
        #endregion

        public ParameterValueEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Value, v => v.ValueTextBox.Text)
                    .DisposeWith(d);
            });
        }
    }
}
