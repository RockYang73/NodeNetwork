using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ActuatorApp.ViewModels.Nodes;
using DynamicData;
using NodeNetwork.Toolkit.Layout.ForceDirected;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ActuatorApp.ViewModels
{
    /// <summary>
    /// 主視窗 ViewModel
    /// </summary>
    public class MainViewModel : ReactiveObject
    {
        /// <summary>
        /// 節點網路
        /// </summary>
        public NetworkViewModel Network { get; }

        /// <summary>
        /// 節點分類集合 (對應 TabControl)
        /// </summary>
        public ObservableCollection<NodeCategory> NodeCategories { get; }

        /// <summary>
        /// 自動佈局命令
        /// </summary>
        public ReactiveCommand<Unit, Unit> AutoLayout { get; }

        public MainViewModel()
        {
            Network = new NetworkViewModel();

            // 建立產品分類 NodeList
            NodeCategories = new ObservableCollection<NodeCategory>
            {
                // Control Box (TC 系列)
                new NodeCategory
                {
                    Name = "Control Box",
                    Nodes = new ObservableCollection<NodeItemViewModel>
                    {
                        new NodeItemViewModel("TC12", () => new TCNodeViewModel("TC12", 2)),
                        new NodeItemViewModel("TC14", () => new TCNodeViewModel("TC14", 3)),
                        new NodeItemViewModel("TC16", () => new TCNodeViewModel("TC16", 4)),
                    }
                },

                // Actuator (TA 系列)
                new NodeCategory
                {
                    Name = "Actuator",
                    Nodes = new ObservableCollection<NodeItemViewModel>
                    {
                        new NodeItemViewModel("TA6", () => new TANodeViewModel("TA6")),
                        new NodeItemViewModel("TA9", () => new TANodeViewModel("TA9")),
                        new NodeItemViewModel("TA12", () => new TANodeViewModel("TA12")),
                    }
                },

                // Controls (TH/TFH 系列)
                new NodeCategory
                {
                    Name = "Controls",
                    SubCategories = new ObservableCollection<NodeSubCategory>
                    {
                        new NodeSubCategory
                        {
                            Name = "Embedded",
                            Nodes = new ObservableCollection<NodeItemViewModel>
                            {
                                new NodeItemViewModel("TFH2", () => new THNodeViewModel("TFH2")),
                                new NodeItemViewModel("TFH6", () => new THNodeViewModel("TFH6")),
                                new NodeItemViewModel("TFH7", () => new THNodeViewModel("TFH7")),
                                new NodeItemViewModel("TFH9", () => new THNodeViewModel("TFH9")),
                                new NodeItemViewModel("TFH15", () => new THNodeViewModel("TFH15")),
                            }
                        },
                        new NodeSubCategory
                        {
                            Name = "Wire Handset",
                            Nodes = new ObservableCollection<NodeItemViewModel>
                            {
                                new NodeItemViewModel("TH1", () => new THNodeViewModel("TH1")),
                                new NodeItemViewModel("TH4", () => new THNodeViewModel("TH4")),
                                new NodeItemViewModel("TH7", () => new THNodeViewModel("TH7")),
                                new NodeItemViewModel("TH7R", () => new THNodeViewModel("TH7R")),
                                new NodeItemViewModel("TH11", () => new THNodeViewModel("TH11")),
                                new NodeItemViewModel("TH17", () => new THNodeViewModel("TH17")),
                                new NodeItemViewModel("TFH35", () => new THNodeViewModel("TFH35")),
                            }
                        },
                        new NodeSubCategory
                        {
                            Name = "Wireless Handset",
                            Nodes = new ObservableCollection<NodeItemViewModel>
                            {
                                new NodeItemViewModel("TH3", () => new THNodeViewModel("TH3")),
                                new NodeItemViewModel("TH8", () => new THNodeViewModel("TH8")),
                                new NodeItemViewModel("TH13", () => new THNodeViewModel("TH13")),
                                new NodeItemViewModel("TH25", () => new THNodeViewModel("TH25")),
                                new NodeItemViewModel("TFH22", () => new THNodeViewModel("TFH22")),
                                new NodeItemViewModel("TFH25", () => new THNodeViewModel("TFH25")),
                                new NodeItemViewModel("TFH27", () => new THNodeViewModel("TFH27")),
                                new NodeItemViewModel("TFH28", () => new THNodeViewModel("TFH28")),
                                new NodeItemViewModel("TFH34", () => new THNodeViewModel("TFH34")),
                            }
                        }
                    }
                },

                // Accessories (TYC/THP 系列)
                new NodeCategory
                {
                    Name = "Accessories",
                    Nodes = new ObservableCollection<NodeItemViewModel>
                    {
                        new NodeItemViewModel("TYC", () => new TYCNodeViewModel("TYC")),
                        new NodeItemViewModel("THP", () => new THPNodeViewModel("THP")),
                    }
                },

                // Power Supply (TP 系列)
                new NodeCategory
                {
                    Name = "Power Supply",
                    Nodes = new ObservableCollection<NodeItemViewModel>
                    {
                        new NodeItemViewModel("TP5", () => new TPNodeViewModel("TP5")),
                        new NodeItemViewModel("TP7", () => new TPNodeViewModel("TP7")),
                    }
                },

                // Battery (TBB 系列)
                new NodeCategory
                {
                    Name = "Battery",
                    Nodes = new ObservableCollection<NodeItemViewModel>
                    {
                        new NodeItemViewModel("TBB3", () => new TBBNodeViewModel("TBB3")),
                        new NodeItemViewModel("BAT1", () => new TBBNodeViewModel("BAT1")),
                    }
                },
            };

            // 自動佈局
            var layouter = new ForceDirectedLayouter();
            AutoLayout = ReactiveCommand.Create(() =>
                layouter.Layout(new Configuration { Network = Network }, 10000));
        }

        /// <summary>
        /// 新增節點到網路
        /// </summary>
        public void AddNode(NodeViewModel node)
        {
            Network.Nodes.Add(node);
        }

        /// <summary>
        /// 新增參數節點
        /// </summary>
        public void AddParameterNode(string paramName)
        {
            var node = new ParameterNodeViewModel(paramName);
            Network.Nodes.Add(node);
        }
    }

    #region 輔助類別

    /// <summary>
    /// 節點分類 (對應 Tab)
    /// </summary>
    public class NodeCategory
    {
        public string Name { get; set; }
        public ObservableCollection<NodeItemViewModel> Nodes { get; set; }
        public ObservableCollection<NodeSubCategory> SubCategories { get; set; }
    }

    /// <summary>
    /// 節點子分類
    /// </summary>
    public class NodeSubCategory
    {
        public string Name { get; set; }
        public ObservableCollection<NodeItemViewModel> Nodes { get; set; }
    }

    /// <summary>
    /// 節點項目 ViewModel
    /// </summary>
    public class NodeItemViewModel
    {
        public string Name { get; set; }
        public Func<NodeViewModel> Factory { get; set; }

        public NodeItemViewModel(string name, Func<NodeViewModel> factory)
        {
            Name = name;
            Factory = factory;
        }
    }

    #endregion
}
