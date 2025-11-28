using NodeNetwork;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ActuatorApp.ViewModels.Nodes
{
    public class GroupNodeViewModel : NodeViewModel
    {
        static GroupNodeViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeView(), typeof(IViewFor<GroupNodeViewModel>));
        }

        /// <summary>
        /// The sub-network contained within this group node.
        /// </summary>
        public NetworkViewModel Subnet { get; }

        public GroupNodeViewModel(NetworkViewModel subnet)
        {
            this.Subnet = subnet;
            this.Name = "Group Node";
        }
    }
}
