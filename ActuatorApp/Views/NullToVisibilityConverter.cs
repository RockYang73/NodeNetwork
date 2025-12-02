using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ActuatorApp.Views
{
    /// <summary>
    /// 將 null 值轉換為 Visibility 的轉換器。
    /// null 或空值會轉換為 Collapsed，非 null 值則轉換為 Visible。
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 將值轉換為 Visibility。
        /// </summary>
        /// <param name="value">要轉換的值</param>
        /// <param name="targetType">目標類型</param>
        /// <param name="parameter">轉換參數 (可選，設為 "Invert" 可反轉邏輯)</param>
        /// <param name="culture">文化資訊</param>
        /// <returns>Visibility 值</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverted = parameter != null && 
                              parameter.ToString().Equals("Invert", StringComparison.OrdinalIgnoreCase);

            bool hasValue = value != null;
            
            // 對於字串，額外檢查是否為空
            if (value is string stringValue)
            {
                hasValue = !string.IsNullOrWhiteSpace(stringValue);
            }

            if (isInverted)
            {
                return hasValue ? Visibility.Collapsed : Visibility.Visible;
            }

            return hasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 將 Visibility 轉換回值 (不支援)。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
