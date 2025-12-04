using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using ActuatorApp.Core.Interfaces;
using ActuatorApp.Core.Products;
using ActuatorApp.Core.Enums;
using ActuatorApp.ViewModels;
using ActuatorApp.ViewModels.Nodes;
using NodeNetwork.ViewModels;

namespace ActuatorApp.Adapters
{
    /// <summary>
    /// NodeNetwork 框架的節點工廠實現
    /// 將核心 ProductDefinition 轉換為 NodeNetwork 節點
    /// </summary>
    public class NodeNetworkNodeFactory : INodeFactory
    {
        private readonly ProductCatalog _catalog;
        private readonly IProductImageService _imageService;
        private readonly Dictionary<string, Func<ProductDefinition, NodeViewModel>> _nodeCreators;

        public NodeNetworkNodeFactory(ProductCatalog catalog) : this(catalog, null)
        {
        }

        public NodeNetworkNodeFactory(ProductCatalog catalog, IProductImageService imageService)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _imageService = imageService;
            _nodeCreators = InitializeNodeCreators();
        }

        private Dictionary<string, Func<ProductDefinition, NodeViewModel>> InitializeNodeCreators()
        {
            return new Dictionary<string, Func<ProductDefinition, NodeViewModel>>
            {
                // Power Supply 節點
                { "TP5", p => new TPNodeViewModel(p.ModelNumber) },
                { "TP7", p => new TPNodeViewModel(p.ModelNumber) },

                // Battery 節點
                { "TBB3", p => new TBBNodeViewModel(p.ModelNumber) },
                { "BAT1", p => new TBBNodeViewModel(p.ModelNumber) },

                // ControlBox 節點
                { "TC12", p => new TCNodeViewModel(p.ModelNumber, GetMotorCount(p)) },
                { "TC14", p => new TCNodeViewModel(p.ModelNumber, GetMotorCount(p)) },
                { "TC16", p => new TCNodeViewModel(p.ModelNumber, GetMotorCount(p)) },

                // Actuator 節點
                { "TA6", p => new TANodeViewModel(p.ModelNumber) },
                { "TA9", p => new TANodeViewModel(p.ModelNumber) },
                { "TA12", p => new TANodeViewModel(p.ModelNumber) },

                // Control 節點 - Embedded 系列
                { "TFH2", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH6", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH7", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH9", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH15", p => new THNodeViewModel(p.ModelNumber) },

                // Control 節點 - Wire Handset 系列
                { "TH1", p => new THNodeViewModel(p.ModelNumber) },
                { "TH4", p => new THNodeViewModel(p.ModelNumber) },
                { "TH7", p => new THNodeViewModel(p.ModelNumber) },
                { "TH7R", p => new THNodeViewModel(p.ModelNumber) },
                { "TH11", p => new THNodeViewModel(p.ModelNumber) },
                { "TH17", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH35", p => new THNodeViewModel(p.ModelNumber) },

                // Control 節點 - Wireless Handset 系列
                { "TH3", p => new THNodeViewModel(p.ModelNumber) },
                { "TH8", p => new THNodeViewModel(p.ModelNumber) },
                { "TH13", p => new THNodeViewModel(p.ModelNumber) },
                { "TH25", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH22", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH25", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH27", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH28", p => new THNodeViewModel(p.ModelNumber) },
                { "TFH34", p => new THNodeViewModel(p.ModelNumber) },

                // Control 節點 - Handheld 系列 (THP)
                { "THP", p => new THPNodeViewModel(p.ModelNumber) },

                // Accessory 節點 - TYC 系列
                { "TYC", p => new TYCNodeViewModel(p.ModelNumber) },
            };
        }

        /// <summary>
        /// 從 ProductDefinition 取得 MotorCount
        /// </summary>
        private static int GetMotorCount(ProductDefinition product)
        {
            if (product?.Properties != null && 
                product.Properties.TryGetValue("MotorCount", out var value))
            {
                if (value is int count) return count;
                if (value is long longCount) return (int)longCount;
                if (int.TryParse(value?.ToString(), out var parsed)) return parsed;
            }
            return 3; // 預設值
        }

        /// <summary>
        /// 根據 ProductDefinition 建立節點 (實作 INodeFactory)
        /// </summary>
        object INodeFactory.CreateNode(ProductDefinition definition)
        {
            return CreateNode(definition);
        }

        /// <summary>
        /// 根據型號名稱建立節點 (實作 INodeFactory)
        /// </summary>
        object INodeFactory.CreateNodeByModel(string modelNumber)
        {
            return CreateNodeByModel(modelNumber);
        }

        /// <summary>
        /// 根據 ProductDefinition 建立節點
        /// </summary>
        public IActuatorNode CreateNode(ProductDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            // 1. 優先使用特定型號的建立器
            if (_nodeCreators.TryGetValue(definition.ModelNumber, out var creator))
            {
                var node = creator(definition);
                if (node is ActuatorNodeViewModel actuatorNode)
                {
                    SetProductImage(actuatorNode, definition.ModelNumber);
                    return actuatorNode;
                }
            }

            // 2. 使用通用類型建立器
            var genericCreator = GetCreatorForType(definition);
            if (genericCreator != null)
            {
                var node = genericCreator(definition);
                if (node is ActuatorNodeViewModel actuatorNode)
                {
                    SetProductImage(actuatorNode, definition.ModelNumber);
                    return actuatorNode;
                }
            }

            throw new NotSupportedException($"不支援的產品型號: {definition.ModelNumber}");
        }

        private Func<ProductDefinition, NodeViewModel> GetCreatorForType(ProductDefinition product)
        {
            switch (product.NodeType)
            {
                case ActuatorApp.Core.Enums.NodeType.PowerSupply:
                    return p => new TPNodeViewModel(p.ModelNumber);
                case ActuatorApp.Core.Enums.NodeType.Battery:
                    return p => new TBBNodeViewModel(p.ModelNumber);
                case ActuatorApp.Core.Enums.NodeType.ControlBox:
                    return p => new TCNodeViewModel(p.ModelNumber, GetMotorCount(p));
                case ActuatorApp.Core.Enums.NodeType.Actuator:
                    return p => new TANodeViewModel(p.ModelNumber);
                case ActuatorApp.Core.Enums.NodeType.Control:
                    if (product.SubCategory == "Handheld")
                        return p => new THPNodeViewModel(p.ModelNumber);
                    return p => new THNodeViewModel(p.ModelNumber);
                case ActuatorApp.Core.Enums.NodeType.Accessory:
                    return p => new TYCNodeViewModel(p.ModelNumber);
                case ActuatorApp.Core.Enums.NodeType.Touch:
                case ActuatorApp.Core.Enums.NodeType.TCS:
                    return p => new THNodeViewModel(p.ModelNumber);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 設定節點的產品圖片
        /// </summary>
        private void SetProductImage(ActuatorNodeViewModel node, string modelNumber)
        {
            if (_imageService == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductImage] ImageService is null for {modelNumber}");
                return;
            }

            var imagePath = _imageService.GetImagePath(modelNumber);
            System.Diagnostics.Debug.WriteLine($"[ProductImage] Model: {modelNumber}, Path: {imagePath ?? "null"}");
            
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[ProductImage] Loading image from: {imagePath}");
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze(); // 允許跨執行緒存取
                    node.ProductImage = bitmap;
                    System.Diagnostics.Debug.WriteLine($"[ProductImage] Successfully loaded image for {modelNumber}, Width={bitmap.PixelWidth}, Height={bitmap.PixelHeight}");
                    System.Diagnostics.Debug.WriteLine($"[ProductImage] node.ProductImage is null? {node.ProductImage == null}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProductImage] Failed to load image for {modelNumber}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ProductImage] Exception: {ex}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProductImage] No image path found for {modelNumber}");
            }
        }

        /// <summary>
        /// 根據型號名稱建立節點
        /// </summary>
        public IActuatorNode CreateNodeByModel(string modelNumber)
        {
            var definition = _catalog.GetProduct(modelNumber);
            if (definition == null)
                throw new ArgumentException($"找不到產品定義: {modelNumber}", nameof(modelNumber));

            return CreateNode(definition);
        }

        /// <summary>
        /// 建立 NodeViewModel（供 NodeNetwork 使用）
        /// </summary>
        public NodeViewModel CreateNodeViewModel(string modelNumber)
        {
            var definition = _catalog.GetProduct(modelNumber);
            if (definition == null)
                throw new ArgumentException($"找不到產品定義: {modelNumber}", nameof(modelNumber));

            // 1. 優先使用特定型號的建立器
            if (_nodeCreators.TryGetValue(modelNumber, out var creator))
            {
                var node = creator(definition);
                if (node is ActuatorNodeViewModel actuatorNode) SetProductImage(actuatorNode, modelNumber);
                return node;
            }

            // 2. 使用通用類型建立器
            var genericCreator = GetCreatorForType(definition);
            if (genericCreator != null)
            {
                var node = genericCreator(definition);
                if (node is ActuatorNodeViewModel actuatorNode) SetProductImage(actuatorNode, modelNumber);
                return node;
            }

            throw new NotSupportedException($"不支援的產品型號: {modelNumber}");
        }

        /// <summary>
        /// 取得所有可用的產品型號
        /// </summary>
        public IEnumerable<string> GetAvailableModels()
        {
            // 只返回硬編碼的列表可能不夠，應該要返回 Catalog 中的所有產品
            // 但既有介面可能依賴於此，暫時保留。
            // 若要支援所有動態產品，這裡應該改為:
            // return _catalog.Products.Select(p => p.ModelNumber);
            return _nodeCreators.Keys; 
        }

        /// <summary>
        /// 檢查是否支援指定型號
        /// </summary>
        public bool IsModelSupported(string modelNumber)
        {
            if (_nodeCreators.ContainsKey(modelNumber)) return true;
            
            var product = _catalog.GetProduct(modelNumber);
            if (product != null)
            {
                return GetCreatorForType(product) != null;
            }
            
            return false;
        }
    }
}
