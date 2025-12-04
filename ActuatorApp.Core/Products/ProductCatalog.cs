using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ActuatorApp.Core.Enums;

namespace ActuatorApp.Core.Products
{
    /// <summary>
    /// 產品目錄 - 管理所有產品定義
    /// </summary>
    public class ProductCatalog
    {
        private readonly List<ProductDefinition> _products = new List<ProductDefinition>();
        private readonly List<CategoryDefinition> _categories = new List<CategoryDefinition>();

        /// <summary>
        /// 所有產品定義
        /// </summary>
        public IReadOnlyList<ProductDefinition> Products => _products.AsReadOnly();

        /// <summary>
        /// 所有分類定義
        /// </summary>
        public IReadOnlyList<CategoryDefinition> Categories => _categories.AsReadOnly();

        /// <summary>
        /// 私有建構函式，使用靜態方法建立
        /// </summary>
        private ProductCatalog() { }

        /// <summary>
        /// 根據型號取得產品定義
        /// </summary>
        public ProductDefinition GetProduct(string modelNumber)
        {
            return _products.FirstOrDefault(p => p.ModelNumber == modelNumber);
        }

        /// <summary>
        /// 根據節點類型取得所有產品
        /// </summary>
        public IEnumerable<ProductDefinition> GetProductsByType(NodeType nodeType)
        {
            return _products.Where(p => p.NodeType == nodeType);
        }

        /// <summary>
        /// 根據分類取得所有產品
        /// </summary>
        public IEnumerable<ProductDefinition> GetProductsByCategory(string category)
        {
            return _products.Where(p => p.Category == category);
        }

        /// <summary>
        /// 從 JSON 檔案合併產品資料 (用於載入 products.json 中的額外型號)
        /// </summary>
        public void MergeFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return;

            try
            {
                var json = File.ReadAllText(jsonPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var data = JsonSerializer.Deserialize<ProductJsonData>(json, options);

                if (data?.Products != null)
                {
                    foreach (var item in data.Products)
                    {
                        // 嘗試解析 NodeType
                        if (!System.Enum.TryParse<NodeType>(item.NodeType, true, out var nodeType))
                        {
                            continue; // 跳過不支援的類型
                        }

                        var existing = _products.FirstOrDefault(p => p.ModelNumber == item.ModelNumber);
                        if (existing != null)
                        {
                            // 更新現有產品圖片
                            if (string.IsNullOrEmpty(existing.ImagePath) && !string.IsNullOrEmpty(item.ImagePath))
                            {
                                existing.ImagePath = item.ImagePath;
                            }
                        }
                        else
                        {
                            // 建立新產品
                            ProductDefinition newProduct = null;
                            switch (nodeType)
                            {
                                case NodeType.ControlBox:
                                    newProduct = CreateControlBox(item.ModelNumber, 2); // 預設 2 馬達
                                    break;
                                case NodeType.Battery:
                                    newProduct = CreateBattery(item.ModelNumber);
                                    break;
                                case NodeType.Actuator:
                                    newProduct = CreateActuator(item.ModelNumber);
                                    break;
                                case NodeType.Control:
                                    newProduct = CreateControl(item.ModelNumber, item.SubCategory);
                                    break;
                                case NodeType.Accessory:
                                    newProduct = CreateAccessory(item.ModelNumber);
                                    break;
                                case NodeType.Touch:
                                case NodeType.TCS:
                                    newProduct = CreateTouch(item.ModelNumber, nodeType);
                                    break;
                            }

                            if (newProduct != null)
                            {
                                newProduct.ImagePath = item.ImagePath;
                                _products.Add(newProduct);
                            }
                        }
                    }

                    // 重建分類
                    _categories.Clear();
                    InitializeDefaultCategories();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error merging products from JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// 從 JSON 檔案載入產品目錄
        /// </summary>
        public static ProductCatalog LoadFromJson(string jsonPath)
        {
            var json = File.ReadAllText(jsonPath);
            return LoadFromJsonString(json);
        }

        /// <summary>
        /// 從 JSON 字串載入產品目錄
        /// </summary>
        public static ProductCatalog LoadFromJsonString(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var data = JsonSerializer.Deserialize<ProductCatalogData>(json, options);
            
            var catalog = new ProductCatalog();
            if (data?.Products != null)
            {
                catalog._products.AddRange(data.Products);
            }
            if (data?.Categories != null)
            {
                catalog._categories.AddRange(data.Categories);
            }
            
            return catalog;
        }

        /// <summary>
        /// 建立預設產品目錄 (包含所有預設產品)
        /// </summary>
        public static ProductCatalog CreateDefault()
        {
            var catalog = new ProductCatalog();
            
            // 建立產品定義
            catalog.InitializeDefaultProducts();
            
            // 建立分類
            catalog.InitializeDefaultCategories();
            
            return catalog;
        }
        
        private class ProductJsonData
        {
            public List<ProductJsonEntry> Products { get; set; }
        }

        private class ProductJsonEntry
        {
            public string ModelNumber { get; set; }
            public string NodeType { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string ImagePath { get; set; }
        }

        /// <summary>
        /// 初始化預設產品
        /// </summary>
        private void InitializeDefaultProducts()
        {
            // ========== Power Supply (TP 系列) ==========
            _products.Add(CreatePowerSupply("TP5"));
            _products.Add(CreatePowerSupply("TP7"));

            // ========== Battery (TBB 系列) ==========
            _products.Add(CreateBattery("TBB3"));
            _products.Add(CreateBattery("BAT1"));

            // ========== Control Box (TC 系列) ==========
            _products.Add(CreateControlBox("TC12", 2));
            _products.Add(CreateControlBox("TC14", 3));
            _products.Add(CreateControlBox("TC16", 4));

            // ========== Actuator (TA 系列) ==========
            _products.Add(CreateActuator("TA6"));
            _products.Add(CreateActuator("TA9"));
            _products.Add(CreateActuator("TA12"));

            // ========== Controls - Embedded ==========
            _products.Add(CreateControl("TFH2", "Embedded"));
            _products.Add(CreateControl("TFH6", "Embedded"));
            _products.Add(CreateControl("TFH7", "Embedded"));
            _products.Add(CreateControl("TFH9", "Embedded"));
            _products.Add(CreateControl("TFH15", "Embedded"));

            // ========== Controls - Wire Handset ==========
            _products.Add(CreateControl("TH1", "Wire Handset"));
            _products.Add(CreateControl("TH4", "Wire Handset"));
            _products.Add(CreateControl("TH7", "Wire Handset"));
            _products.Add(CreateControl("TH7R", "Wire Handset"));
            _products.Add(CreateControl("TH11", "Wire Handset"));
            _products.Add(CreateControl("TH17", "Wire Handset"));
            _products.Add(CreateControl("TFH35", "Wire Handset"));

            // ========== Controls - Wireless Handset ==========
            _products.Add(CreateControl("TH3", "Wireless Handset"));
            _products.Add(CreateControl("TH8", "Wireless Handset"));
            _products.Add(CreateControl("TH13", "Wireless Handset"));
            _products.Add(CreateControl("TH25", "Wireless Handset"));
            _products.Add(CreateControl("TFH22", "Wireless Handset"));
            _products.Add(CreateControl("TFH25", "Wireless Handset"));
            _products.Add(CreateControl("TFH27", "Wireless Handset"));
            _products.Add(CreateControl("TFH28", "Wireless Handset"));
            _products.Add(CreateControl("TFH34", "Wireless Handset"));

            // ========== Controls - Handheld ==========
            _products.Add(CreateHandheldControl("THP"));

            // ========== Accessories (TYC 系列) ==========
            _products.Add(CreateAccessory("TYC"));
        }

        /// <summary>
        /// 初始化預設分類
        /// </summary>
        private void InitializeDefaultCategories()
        {
            _categories.Add(new CategoryDefinition
            {
                Name = "Control Box",
                Order = 1,
                Description = "控制盒",
                Products = _products.Where(p => p.Category == "Control Box").ToList()
            });

            _categories.Add(new CategoryDefinition
            {
                Name = "Actuator",
                Order = 2,
                Description = "電動推桿",
                Products = _products.Where(p => p.Category == "Actuator").ToList()
            });

            _categories.Add(new CategoryDefinition
            {
                Name = "Controls",
                Order = 3,
                Description = "手控器",
                SubCategories = new List<SubCategoryDefinition>
                {
                    new SubCategoryDefinition
                    {
                        Name = "Embedded",
                        Order = 1,
                        Products = _products.Where(p => p.Category == "Controls" && p.SubCategory == "Embedded").ToList()
                    },
                    new SubCategoryDefinition
                    {
                        Name = "Wire Handset",
                        Order = 2,
                        Products = _products.Where(p => p.Category == "Controls" && p.SubCategory == "Wire Handset").ToList()
                    },
                    new SubCategoryDefinition
                    {
                        Name = "Wireless Handset",
                        Order = 3,
                        Products = _products.Where(p => p.Category == "Controls" && p.SubCategory == "Wireless Handset").ToList()
                    },
                    new SubCategoryDefinition
                    {
                        Name = "Handheld",
                        Order = 4,
                        Products = _products.Where(p => p.Category == "Controls" && p.SubCategory == "Handheld").ToList()
                    }
                }
            });

            _categories.Add(new CategoryDefinition
            {
                Name = "Accessories",
                Order = 4,
                Description = "配件 (延長線/分接線)",
                Products = _products.Where(p => p.Category == "Accessories").ToList()
            });

            _categories.Add(new CategoryDefinition
            {
                Name = "Power Supply",
                Order = 5,
                Description = "電源供應器",
                Products = _products.Where(p => p.Category == "Power Supply").ToList()
            });

            _categories.Add(new CategoryDefinition
            {
                Name = "Battery",
                Order = 6,
                Description = "電池",
                Products = _products.Where(p => p.Category == "Battery").ToList()
            });

            _categories.Add(new CategoryDefinition
            {
                Name = "T-touch",
                Order = 7,
                Description = "觸控系列",
                Products = _products.Where(p => p.Category == "T-touch").ToList()
            });
        }

        #region 產品建立輔助方法

        private static ProductDefinition CreatePowerSupply(string model)
        {
            return new ProductDefinition
            {
                ModelNumber = model,
                NodeType = NodeType.PowerSupply,
                Category = "Power Supply",
                BackgroundColor = "#4CAF50",
                Outputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "Out", PortType = PortType.Power, Description = "電流輸出" }
                }
            };
        }

        private static ProductDefinition CreateBattery(string model)
        {
            return new ProductDefinition
            {
                ModelNumber = model,
                NodeType = NodeType.Battery,
                Category = "Battery",
                BackgroundColor = "#8BC34A",
                Inputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "In", PortType = PortType.Power, Description = "電流輸入" }
                },
                Outputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "Out", PortType = PortType.Power, Description = "電流輸出" }
                }
            };
        }

        private static ProductDefinition CreateControlBox(string model, int motorCount)
        {
            var product = new ProductDefinition
            {
                ModelNumber = model,
                NodeType = NodeType.ControlBox,
                Category = "Control Box",
                BackgroundColor = "#2196F3",
                Properties = new Dictionary<string, object>
                {
                    { "MotorCount", motorCount }
                },
                Inputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "Power", PortType = PortType.Power, Description = "電流輸入" },
                    new PortDefinition { Name = "H1", PortType = PortType.BinaryData, Description = "手控器信號輸入" }
                },
                Outputs = new List<PortDefinition>
                {
                    new PortDefinition 
                    { 
                        Name = "M", 
                        PortType = PortType.BinaryData, 
                        IsDynamic = true, 
                        Count = motorCount,
                        Description = "推桿控制輸出"
                    }
                }
            };
            return product;
        }

        private static ProductDefinition CreateActuator(string model)
        {
            return new ProductDefinition
            {
                ModelNumber = model,
                NodeType = NodeType.Actuator,
                Category = "Actuator",
                BackgroundColor = "#FF9800",
                Inputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "TC", PortType = PortType.BinaryData, Description = "控制信號輸入" },
                    new PortDefinition { Name = "Code", PortType = PortType.Parameter, Description = "推桿編碼" },
                    new PortDefinition { Name = "Stroke", PortType = PortType.Parameter, Description = "行程設定" }
                }
            };
        }

        private static ProductDefinition CreateControl(string model, string subCategory)
        {
            return new ProductDefinition
            {
                ModelNumber = model,
                NodeType = NodeType.Control,
                Category = "Controls",
                SubCategory = subCategory,
                BackgroundColor = "#9C27B0",
                Properties = new Dictionary<string, object>
                {
                    { "SupportedProtocols", new[] { "GPIO", "Scan", "TBUS" } }
                },
                Outputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "TC", PortType = PortType.BinaryData, Description = "控制信號輸出" }
                }
            };
        }

        private static ProductDefinition CreateHandheldControl(string model)
        {
            return new ProductDefinition
            {
                ModelNumber = model,
                NodeType = NodeType.Control,
                Category = "Controls",
                SubCategory = "Handheld",
                BackgroundColor = "#9C27B0",
                Outputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "TC", PortType = PortType.BinaryData, Description = "控制信號輸出" }
                }
            };
        }

        private static ProductDefinition CreateAccessory(string model)
        {
            return new ProductDefinition
            {
                ModelNumber = model,
                NodeType = NodeType.Accessory,
                Category = "Accessories",
                BackgroundColor = "#607D8B",
                Description = "延長線/分接線",
                Inputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "IN1", PortType = PortType.BinaryData, Description = "信號輸入 1" },
                    new PortDefinition { Name = "IN2", PortType = PortType.BinaryData, Description = "信號輸入 2" }
                },
                Outputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "Out", PortType = PortType.BinaryData, Description = "信號輸出" }
                }
            };
        }

        private static ProductDefinition CreateTouch(string model, NodeType type)
        {
            return new ProductDefinition
            {
                ModelNumber = model,
                NodeType = type,
                Category = "T-touch",
                BackgroundColor = "#E91E63",
                Outputs = new List<PortDefinition>
                {
                    new PortDefinition { Name = "Out", PortType = PortType.BinaryData, Description = "信號輸出" }
                }
            };
        }

        #endregion
    }

    /// <summary>
    /// JSON 反序列化用的資料結構
    /// </summary>
    internal class ProductCatalogData
    {
        public List<ProductDefinition> Products { get; set; }
        public List<CategoryDefinition> Categories { get; set; }
    }
}
