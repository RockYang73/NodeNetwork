using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ActuatorApp.ViewModels.Nodes;

namespace ActuatorApp.Views.Converters
{
    /// <summary>
    /// Given an object o and a type t, return Visible if o is of type t, else return Collapsed.
    /// Special case: if checking for GroupNodeViewModel, also returns Visible for ContainerGroupNodeViewModel.
    /// </summary>
    public class TypeVisibilityCheck : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            
            var paramType = (Type)parameter;
            
            // Special case: if checking for GroupNodeViewModel, also accept ContainerGroupNodeViewModel
            if (paramType == typeof(GroupNodeViewModel))
            {
                return (value is GroupNodeViewModel || value is ContainerGroupNodeViewModel) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
            
            return paramType.IsInstanceOfType(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
