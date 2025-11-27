using ActuatorApp.Views.Editors;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Editors
{
    /// <summary>
    /// 參數值編輯器 ViewModel
    /// </summary>
    public class ParameterValueEditorViewModel : ValueEditorViewModel<string>
    {
        static ParameterValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(
                () => new ParameterValueEditorView(),
                typeof(IViewFor<ParameterValueEditorViewModel>));
        }

        public ParameterValueEditorViewModel()
        {
            Value = string.Empty;
        }
    }
}
