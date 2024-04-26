using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CacheTower.Internal
{
	internal static class WildcardExtensions
	{
		// Supports both "*" and "?"
		public static bool ContainsWildcard(this string value, string wildcard) => Regex.IsMatch(value, "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$");
	}
}
