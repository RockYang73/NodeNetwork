using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActuatorApp.ViewModels;
using ReactiveUI;

namespace ActuatorApp.Views
{
    public partial class ActuatorPortView : UserControl, IViewFor<ActuatorPortViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                typeof(ActuatorPortViewModel), typeof(ActuatorPortView),
                new PropertyMetadata(null));

        public ActuatorPortViewModel ViewModel
        {
            get => (ActuatorPortViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ActuatorPortViewModel)value;
        }
        #endregion

        #region Template Resource Keys
        public const string PowerPortTemplateKey = "PowerPortTemplate";
        public const string BinaryDataPortTemplateKey = "BinaryDataPortTemplate";
        public const string ParameterPortTemplateKey = "ParameterPortTemplate";
        #endregion

        public ActuatorPortView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel)
                    .BindTo(this, v => v.PortView.ViewModel)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.PortType, v => v.PortView.Template,
                    GetTemplateFromPortType).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.IsMirrored, v => v.PortView.RenderTransform,
                    isMirrored => new ScaleTransform(isMirrored ? -1.0 : 1.0, 1.0))
                    .DisposeWith(d);
            });
        }

        public ControlTemplate GetTemplateFromPortType(PortType type)
        {
            switch (type)
            {
                case PortType.Power:
                    return (ControlTemplate)Resources[PowerPortTemplateKey];
                case PortType.BinaryData:
                    return (ControlTemplate)Resources[BinaryDataPortTemplateKey];
                case PortType.Parameter:
                    return (ControlTemplate)Resources[ParameterPortTemplateKey];
                default:
                    return (ControlTemplate)Resources[BinaryDataPortTemplateKey];
            }
        }
    }
}
