using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Android.Graphics;
using AApplication = Android.App.Application;

namespace Xamarin.Forms.Platform.Android
{
	public static class FontExtensions
	{
		static readonly Dictionary<Tuple<string, FontAttributes>, Typeface> Typefaces = new Dictionary<Tuple<string, FontAttributes>, Typeface>();

		static readonly Lazy<Regex> s_loadFromAssets = new Lazy<Regex>(() => new Regex(@"\w+\.((ttf)|(otf))\#\w*"));

		static Typeface s_defaultTypeface;

		public static float ToScaledPixel(this Font self)
		{
			if (self.IsDefault)
				return 14;

			if (self.UseNamedSize)
			{
				switch (self.NamedSize)
				{
					case NamedSize.Micro:
						return 10;

					case NamedSize.Small:
						return 12;

					case NamedSize.Default:
					case NamedSize.Medium:
						return 14;

					case NamedSize.Large:
						return 18;
				}
			}

			return (float)self.FontSize;
		}

		public static Typeface ToTypeface(this Font self)
		{
			if (self.IsDefault)
				return s_defaultTypeface ?? (s_defaultTypeface = Typeface.Default);

			var key = new Tuple<string, FontAttributes>(self.FontFamily, self.FontAttributes);
			Typeface result;
			if (Typefaces.TryGetValue(key, out result))
				return result;

			if (self.FontFamily == null)
			{
				var style = ToTypefaceStyle(self.FontAttributes);
				result = Typeface.Create(Typeface.Default, style);
			}
			else if (s_loadFromAssets.Value.IsMatch(self.FontFamily))
			{
				result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(self.FontFamily));
			}
			else
			{
				var style = ToTypefaceStyle(self.FontAttributes);
				result = Typeface.Create(self.FontFamily, style);
			}
			return (Typefaces[key] = result);
		}

		internal static bool IsDefault(this IFontElement self)
		{
			return self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) && self.FontAttributes == FontAttributes.None;
		}

		internal static Typeface ToTypeface(this IFontElement self)
		{
			if (self.IsDefault())
				return s_defaultTypeface ?? (s_defaultTypeface = Typeface.Default);

			var key = new Tuple<string, FontAttributes>(self.FontFamily, self.FontAttributes);
			Typeface result;
			if (Typefaces.TryGetValue(key, out result))
				return result;

			if (self.FontFamily == null)
			{
				var style = ToTypefaceStyle(self.FontAttributes);
				result = Typeface.Create(Typeface.Default, style);
			}
			else if (s_loadFromAssets.Value.IsMatch(self.FontFamily))
			{
				result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(self.FontFamily));
			}
			else
			{
				var style = ToTypefaceStyle(self.FontAttributes);
				result = Typeface.Create(self.FontFamily, style);
			}
			return (Typefaces[key] = result);
		}

		public static TypefaceStyle ToTypefaceStyle(FontAttributes attrs)
		{
			var style = TypefaceStyle.Normal;
			if ((attrs & (FontAttributes.Bold | FontAttributes.Italic)) == (FontAttributes.Bold | FontAttributes.Italic))
				style = TypefaceStyle.BoldItalic;
			else if ((attrs & FontAttributes.Bold) != 0)
				style = TypefaceStyle.Bold;
			else if ((attrs & FontAttributes.Italic) != 0)
				style = TypefaceStyle.Italic;
			return style;
		}

		static string FontNameToFontFile(string fontFamily)
		{
			int hashtagIndex = fontFamily.IndexOf('#');
			if (hashtagIndex >= 0)
				return fontFamily.Substring(0, hashtagIndex);

			throw new InvalidOperationException($"Can't parse the {nameof(fontFamily)} {fontFamily}");
		}
	}
}