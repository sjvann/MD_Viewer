using System.Globalization;

namespace MD_Viewer.Converters;

/// <summary>
/// 布林值轉文字顏色轉換器
/// </summary>
public class BoolToTextColorConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is bool isEnabled)
		{
			return isEnabled ? Colors.Black : Colors.Gray;
		}

		return Colors.Black;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

