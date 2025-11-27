using System;
using System.Collections.Generic;
using ActuatorApp.Core.Interfaces;
using ActuatorApp.Core.Products;
using ActuatorApp.Core.Enums;
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
        private readonly Dictionary<string, Func<ProductDefinition, NodeViewModel>> _nodeCreators;

        public NodeNetworkNodeFactory(ProductCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
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

            if (_nodeCreators.TryGetValue(definition.ModelNumber, out var creator))
            {
                var node = creator(definition);
                if (node is IActuatorNode actuatorNode)
                {
                    return actuatorNode;
                }
            }

            throw new NotSupportedException($"不支援的產品型號: {definition.ModelNumber}");
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

            if (_nodeCreators.TryGetValue(modelNumber, out var creator))
            {
                return creator(definition);
            }

            throw new NotSupportedException($"不支援的產品型號: {modelNumber}");
        }

        /// <summary>
        /// 取得所有可用的產品型號
        /// </summary>
        public IEnumerable<string> GetAvailableModels()
        {
            return _nodeCreators.Keys;
        }

        /// <summary>
        /// 檢查是否支援指定型號
        /// </summary>
        public bool IsModelSupported(string modelNumber)
        {
            return _nodeCreators.ContainsKey(modelNumber);
        }
    }
}
