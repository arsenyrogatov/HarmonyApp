﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HarmonyApp.Converters
{
    public class BoolFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? FontWeights.Normal : FontWeights.Bold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (FontWeight)value == FontWeights.Bold;
        }
    }
}
