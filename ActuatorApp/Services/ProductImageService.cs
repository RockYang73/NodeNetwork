using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ActuatorApp.Core.Interfaces;

namespace ActuatorApp.Services
{
    /// <summary>
    /// 產品圖片服務實作 - 從 JSON 配置檔載入圖片對應關係
    /// </summary>
    public class ProductImageService : IProductImageService
    {
        private readonly string _basePath;
        private readonly string _defaultImage;
        private readonly Dictionary<string, string> _imageMapping;
        private readonly bool _useDefaultImage;

        /// <summary>
        /// 建立產品圖片服務
        /// </summary>
        /// <param name="configPath">JSON 配置檔路徑 (products.json)</param>
        /// <param name="assetsBasePath">Assets 基礎路徑</param>
        public ProductImageService(string configPath, string assetsBasePath = null)
        {
            _imageMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _basePath = assetsBasePath ?? GetDefaultAssetsPath();
            
            System.Diagnostics.Debug.WriteLine($"[ProductImageService] Config path: {configPath}");
            System.Diagnostics.Debug.WriteLine($"[ProductImageService] Base path: {_basePath}");
            System.Diagnostics.Debug.WriteLine($"[ProductImageService] Config exists: {File.Exists(configPath)}");
            
            if (File.Exists(configPath))
            {
                var config = LoadConfig(configPath);
                _defaultImage = config.DefaultImage;
                _useDefaultImage = !string.IsNullOrEmpty(_defaultImage);
                
                foreach (var product in config.Products)
                {
                    if (!string.IsNullOrEmpty(product.ModelNumber) && 
                        !string.IsNullOrEmpty(product.ImagePath))
                    {
                        _imageMapping[product.ModelNumber] = product.ImagePath;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[ProductImageService] Loaded {_imageMapping.Count} product mappings");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProductImageService] Config file not found!");
            }
        }

        /// <summary>
        /// 取得預設 Assets 路徑
        /// </summary>
        private static string GetDefaultAssetsPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "Assets", "Products");
        }

        /// <summary>
        /// 載入 JSON 配置檔
        /// </summary>
        private static ProductImageConfig LoadConfig(string configPath)
        {
            try
            {
                var json = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<ProductImageConfig>(json, options) 
                       ?? new ProductImageConfig();
            }
            catch
            {
                return new ProductImageConfig();
            }
        }

        /// <summary>
        /// 根據產品型號取得圖片完整路徑
        /// </summary>
        public string GetImagePath(string modelNumber)
        {
            if (string.IsNullOrEmpty(modelNumber))
                return null;

            // 從對應表查詢
            if (_imageMapping.TryGetValue(modelNumber, out var relativePath))
            {
                // 確保路徑分隔符號正確
                var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_basePath, normalizedPath);
                System.Diagnostics.Debug.WriteLine($"[ProductImageService] GetImagePath({modelNumber}) => {fullPath}, Exists: {File.Exists(fullPath)}");
                if (File.Exists(fullPath))
                    return fullPath;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProductImageService] GetImagePath({modelNumber}) => Not found in mapping");
            }

            // 嘗試自動推斷路徑 (依 NodeType 資料夾)
            var inferredPath = TryInferImagePath(modelNumber);
            if (inferredPath != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductImageService] Inferred path for {modelNumber}: {inferredPath}");
                return inferredPath;
            }

            // 回傳預設圖片 (設定檔中的 DefaultImage)
            if (_useDefaultImage)
            {
                var defaultPath = GetDefaultImagePath();
                if (defaultPath != null) return defaultPath;
            }

            // 若都沒有，則回傳 NA.png
            var naPath = Path.Combine(_basePath, "NA.png");
            if (File.Exists(naPath))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductImageService] Using NA.png for {modelNumber}");
                return naPath;
            }

            return null;
        }

        /// <summary>
        /// 嘗試自動推斷圖片路徑
        /// </summary>
        private string TryInferImagePath(string modelNumber)
        {
            // 根據型號前綴推斷資料夾
            var folders = new[] { "Controlbox", "Control", "Actuator", "T-touch", "PowerSupply", "Accessories" };
            
            foreach (var folder in folders)
            {
                var path = Path.Combine(_basePath, folder, $"{modelNumber}.png");
                if (File.Exists(path))
                    return path;
                
                // 嘗試大寫 PNG
                path = Path.Combine(_basePath, folder, $"{modelNumber}.PNG");
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        /// <summary>
        /// 檢查指定型號的產品圖片是否存在
        /// </summary>
        public bool ImageExists(string modelNumber)
        {
            var path = GetImagePath(modelNumber);
            return path != null && File.Exists(path);
        }

        /// <summary>
        /// 取得預設圖片路徑
        /// </summary>
        public string GetDefaultImagePath()
        {
            if (!_useDefaultImage)
                return null;

            var defaultPath = Path.Combine(_basePath, _defaultImage);
            return File.Exists(defaultPath) ? defaultPath : null;
        }

        #region JSON 配置結構

        private class ProductImageConfig
        {
            public string Version { get; set; } = "1.0";
            public string BasePath { get; set; } = "Assets/Products";
            public string DefaultImage { get; set; }
            public List<ProductImageEntry> Products { get; set; } = new List<ProductImageEntry>();
        }

        private class ProductImageEntry
        {
            public string ModelNumber { get; set; }
            public string NodeType { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string ImagePath { get; set; }
        }

        #endregion
    }
}
