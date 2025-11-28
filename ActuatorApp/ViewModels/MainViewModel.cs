using System;
using System.Collections.ObjectModel;
using System.Diagnostics; // Added for Debug.WriteLine
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ActuatorApp.Adapters;
using ActuatorApp.Core.Enums;
using ActuatorApp.Core.Products;
using ActuatorApp.Core.Interfaces; // Added
using ActuatorApp.Services; // Added
using ActuatorApp.ViewModels.Nodes;
using DynamicData;
using NodeNetwork.Toolkit.Group; // Added for NodeGrouper
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
        /// 產品目錄 (從 Core 層讀取)
        /// </summary>
        private readonly ProductCatalog _catalog;

        /// <summary>
        /// 節點工廠 (NodeNetwork 適配器)
        /// </summary>
        private readonly NodeNetworkNodeFactory _nodeFactory;

        /// <summary>
        /// 資料庫產品存取介面
        /// </summary>
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// 自動連接服務
        /// </summary>
        private readonly IAutoConnectService _autoConnectService;

        /// <summary>
        /// 節點群組化工具
        /// </summary>
        private readonly NodeGrouper _grouper;

        /// <summary>
        /// 節點網路
        /// </summary>
        public NetworkViewModel Network { get; }

        /// <summary>
        /// 節點分類集合 (對應 TabControl)
        /// </summary>
        public ObservableCollection<NodeCategory> NodeCategories { get; }

        /// <summary>
        /// 目前選中的節點分類索引
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
        }
        private int _selectedTabIndex;

        /// <summary>
        /// 自動佈局命令
        /// </summary>
        public ReactiveCommand<Unit, Unit> AutoLayout { get; }

        /// <summary>
        /// 載入產品資料命令 (從資料庫)
        /// </summary>
        public ReactiveCommand<Unit, Unit> LoadProductDataCommand { get; }

        /// <summary>
        /// 群組節點命令
        /// </summary>
        public ReactiveCommand<Unit, Unit> GroupNodesCommand { get; }

        public MainViewModel(IProductRepository productRepository, IAutoConnectService autoConnectService) // Modified constructor
        {
            _productRepository = productRepository; // Store injected repository
            _autoConnectService = autoConnectService; // Store injected service

            // 從 Core 層載入產品目錄
            _catalog = ProductCatalog.CreateDefault();
            _nodeFactory = new NodeNetworkNodeFactory(_catalog);

            Network = new NetworkViewModel();

            // 根據 ProductCatalog 建立 NodeCategories
            NodeCategories = BuildNodeCategoriesFromCatalog();

            // 設定預設選中 "Power Supply" 分類 (索引 4)
            SelectedTabIndex = 4;

            // 初始化 NodeGrouper
            _grouper = new NodeGrouper
            {
                GroupNodeFactory = (subnet) => new GroupNodeViewModel(subnet),
                SubNetworkFactory = () => new NetworkViewModel(),
                EntranceNodeFactory = () => new NodeViewModel { Name = "Group Input" },
                ExitNodeFactory = () => new NodeViewModel { Name = "Group Output" },
                IOBindingFactory = (groupNode, entranceNode, exitNode) =>
                    new ValueNodeGroupIOBinding(groupNode, entranceNode, exitNode)
            };

            // 群組節點命令
            GroupNodesCommand = ReactiveCommand.Create(() =>
            {
                var selectedNodes = Network.SelectedNodes.Items.ToList();
                if (selectedNodes.Any())
                {
                    _grouper.MergeIntoGroup(Network, selectedNodes);
                }
            });

            // 自動佈局
            var layouter = new ForceDirectedLayouter();
            AutoLayout = ReactiveCommand.Create(() =>
                layouter.Layout(new Configuration { Network = Network }, 10000));
            
            // 初始化載入產品資料命令
            LoadProductDataCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                Debug.WriteLine("Loading Control data from repository...");
                var controls = await _productRepository.GetControlsAsync();
                foreach (var control in controls)
                {
                    Debug.WriteLine($"- Loaded Control: Id={control.Id}, Name={control.Name}, ImgPath={control.ImgPath}");
                }
                Debug.WriteLine("Finished loading Control data.");
            });

            // 在 ViewModel 初始化時執行載入命令
            LoadProductDataCommand.Execute().Subscribe();
        }

        /// <summary>
        /// 根據 ProductCatalog 建立 UI 分類結構
        /// </summary>
        private ObservableCollection<NodeCategory> BuildNodeCategoriesFromCatalog()
        {
            var categories = new ObservableCollection<NodeCategory>();

            foreach (var categoryDef in _catalog.Categories)
            {
                var category = new NodeCategory
                {
                    Name = categoryDef.Name
                };

                // 如果有子分類
                if (categoryDef.SubCategories != null && categoryDef.SubCategories.Any())
                {
                    category.SubCategories = new ObservableCollection<NodeSubCategory>();

                    foreach (var subCategoryDef in categoryDef.SubCategories)
                    {
                        var subCategory = new NodeSubCategory
                        {
                            Name = subCategoryDef.Name,
                            Nodes = new ObservableCollection<NodeItemViewModel>()
                        };

                        foreach (var product in subCategoryDef.Products)
                        {
                            if (_nodeFactory.IsModelSupported(product.ModelNumber))
                            {
                                subCategory.Nodes.Add(new NodeItemViewModel(
                                    product.DisplayName ?? product.ModelNumber,
                                    () => _nodeFactory.CreateNodeViewModel(product.ModelNumber)
                                ));
                            }
                        }

                        if (subCategory.Nodes.Any())
                        {
                            category.SubCategories.Add(subCategory);
                        }
                    }
                }
                else
                {
                    // 沒有子分類，直接列出產品
                    category.Nodes = new ObservableCollection<NodeItemViewModel>();

                    foreach (var product in categoryDef.Products)
                    {
                        if (_nodeFactory.IsModelSupported(product.ModelNumber))
                        {
                            category.Nodes.Add(new NodeItemViewModel(
                                product.DisplayName ?? product.ModelNumber,
                                () => _nodeFactory.CreateNodeViewModel(product.ModelNumber)
                            ));
                        }
                    }
                }

                // 只加入有產品的分類
                if ((category.Nodes != null && category.Nodes.Any()) ||
                    (category.SubCategories != null && category.SubCategories.Any()))
                {
                    categories.Add(category);
                }
            }

            return categories;
        }

        /// <summary>
        /// 新增節點到網路
        /// </summary>
        public void AddNode(NodeViewModel node)
        {
            Network.Nodes.Add(node);
            _autoConnectService.TryAutoConnect(Network, node);
        }

        /// <summary>
        /// 根據型號新增節點
        /// </summary>
        public void AddNodeByModel(string modelNumber)
        {
            var node = _nodeFactory.CreateNodeViewModel(modelNumber);
            AddNode(node); // 改為呼叫 AddNode 以觸發自動連接
        }

        /// <summary>
        /// 新增參數節點
        /// </summary>
        public void AddParameterNode(string paramName)
        {
            var node = new ParameterNodeViewModel(paramName);
            AddNode(node); // 改為呼叫 AddNode 以觸發自動連接
        }

        /// <summary>
        /// 取得產品目錄
        /// </summary>
        public ProductCatalog GetCatalog() => _catalog;
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
