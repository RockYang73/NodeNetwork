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
        /// 產品圖片服務
        /// </summary>
        private readonly IProductImageService _productImageService;

        /// <summary>
        /// 節點群組化工具
        /// </summary>
        private readonly NodeGrouper _grouper;

        #region Connection & Program Properties

        public ObservableCollection<string> ComPorts { get; }

        private string _selectedComPort;
        public string SelectedComPort
        {
            get => _selectedComPort;
            set => this.RaiseAndSetIfChanged(ref _selectedComPort, value);
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isConnected, value);
                this.RaisePropertyChanged(nameof(ConnectionStatus));
                this.RaisePropertyChanged(nameof(ConnectButtonText));
                this.RaisePropertyChanged(nameof(ConnectionStatusColor));
            }
        }

        public string ConnectionStatus => IsConnected ? "Connected" : "Disconnected";
        public string ConnectButtonText => IsConnected ? "Disconnect" : "Connect";
        public string ConnectionStatusColor => IsConnected ? "#4CAF50" : "#F44336"; // Green / Red

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set => this.RaiseAndSetIfChanged(ref _progressValue, value);
        }

        #endregion

        #region Commands
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> ProgramCommand { get; }
        public ReactiveCommand<Unit, Unit> ReadFromMcuCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadFileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveFileCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateSoiCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateHexCommand { get; }
        #endregion

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

        /// <summary>
        /// 解散群組命令
        /// </summary>
        public ReactiveCommand<Unit, Unit> UngroupNodesCommand { get; }

        /// <summary>
        /// 進入群組命令
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenGroupCommand { get; }

        public MainViewModel(IProductRepository productRepository, IAutoConnectService autoConnectService, IProductImageService productImageService) // Modified constructor
        {
            _productRepository = productRepository; // Store injected repository
            _autoConnectService = autoConnectService; // Store injected service
            _productImageService = productImageService; // Store injected image service

            // 從 Core 層載入產品目錄
            _catalog = ProductCatalog.CreateDefault();
            
            // 從 JSON 合併額外產品定義
            string jsonPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "products.json");
            _catalog.MergeFromJson(jsonPath);

            _nodeFactory = new NodeNetworkNodeFactory(_catalog, _productImageService);

            Network = new NetworkViewModel();

            // 根據 ProductCatalog 建立 NodeCategories
            NodeCategories = BuildNodeCategoriesFromCatalog();

            // 設定預設選中 "Power Supply" 分類 (索引 4)
            SelectedTabIndex = 4;

            // Connection & Program Init
            ComPorts = new ObservableCollection<string> { "COM1", "COM2", "COM3", "COM4" };
            SelectedComPort = ComPorts.FirstOrDefault();

            ConnectCommand = ReactiveCommand.Create(() =>
            {
                IsConnected = !IsConnected;
                Debug.WriteLine($"Connection Toggled: {IsConnected}");
            });

            ProgramCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                Debug.WriteLine("Programming...");
                ProgressValue = 0;
                for(int i=0; i<=100; i+=10)
                {
                    ProgressValue = i;
                    await System.Threading.Tasks.Task.Delay(50);
                }
                Debug.WriteLine("Programmed!");
            });

            ReadFromMcuCommand = ReactiveCommand.Create(() =>
            {
                Debug.WriteLine("Reading from MCU...");
            });

                        LoadFileCommand = ReactiveCommand.Create(() => Debug.WriteLine("Load File"));

                        SaveFileCommand = ReactiveCommand.Create(() => Debug.WriteLine("Save File"));

                        GenerateSoiCommand = ReactiveCommand.Create(() => Debug.WriteLine("Generate SOI"));

                        GenerateHexCommand = ReactiveCommand.Create(() => Debug.WriteLine("Generate HEX"));

            

            

                        // 初始化 NodeGrouper

                        _grouper = new NodeGrouper

                        {

                            GroupNodeFactory = (subnet) => new GroupNodeViewModel(subnet),

                            SubNetworkFactory = () => new NetworkViewModel(),

                            EntranceNodeFactory = () => new NodeViewModel { Name = "Group Input" },

                            ExitNodeFactory = () => new NodeViewModel { Name = "Group Output" },

                            IOBindingFactory = (groupNode, entranceNode, exitNode) =>

                                new ActuatorGroupIOBinding(groupNode, entranceNode, exitNode)

                        };

            

                        // 群組節點命令

                        var canGroup = this.WhenAnyObservable(vm => vm.Network.SelectedNodes.CountChanged).Select(c => c > 1);

                        GroupNodesCommand = ReactiveCommand.Create(() =>

                        {

                            var selectedNodes = Network.SelectedNodes.Items.ToList();

                            if (selectedNodes.Any())

                            {

                                var binding = _grouper.MergeIntoGroup(Network, selectedNodes);

                                if (binding.GroupNode is GroupNodeViewModel groupVm)

                                {

                                    groupVm.IOBinding = binding;

                                }

                            }

                        }, canGroup);

            

                        // 判斷是否選中了一個群組節點

                        var isGroupNodeSelected = this.WhenAnyValue(vm => vm.Network)

                            .Select(net => net.SelectedNodes.Connect())

                            .Switch()

                            .Select(_ => Network.SelectedNodes.Count == 1 && Network.SelectedNodes.Items.First() is GroupNodeViewModel);

            

                        // 解散群組命令

                        UngroupNodesCommand = ReactiveCommand.Create(() =>

                        {

                            var selectedGroupNode = (GroupNodeViewModel)Network.SelectedNodes.Items.First();

                            if (selectedGroupNode.IOBinding != null)

                            {

                                _grouper.Ungroup(selectedGroupNode.IOBinding);

                            }

                        }, isGroupNodeSelected);

            

                        // 進入群組命令

                        OpenGroupCommand = ReactiveCommand.Create(() =>

                        {

                            var selectedGroupNode = (GroupNodeViewModel)Network.SelectedNodes.Items.First();

                            // TODO: Implement navigation logic (NetworkStack)

                            System.Diagnostics.Debug.WriteLine($"Opening group: {selectedGroupNode.Name}");

                        }, isGroupNodeSelected);

            

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
                                var imagePath = _productImageService.GetImagePath(product.ModelNumber);
                                subCategory.Nodes.Add(new NodeItemViewModel(
                                    product.DisplayName ?? product.ModelNumber,
                                    imagePath,
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
                            var imagePath = _productImageService.GetImagePath(product.ModelNumber);
                            category.Nodes.Add(new NodeItemViewModel(
                                product.DisplayName ?? product.ModelNumber,
                                imagePath,
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
        public string ImagePath { get; set; }
        public Func<NodeViewModel> Factory { get; set; }

        public NodeItemViewModel(string name, string imagePath, Func<NodeViewModel> factory)
        {
            Name = name;
            ImagePath = imagePath;
            Factory = factory;
        }
    }

    #endregion
}
