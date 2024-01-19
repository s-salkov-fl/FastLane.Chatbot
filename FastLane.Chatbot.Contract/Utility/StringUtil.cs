using System.Text.RegularExpressions;

namespace FastLane.Chatbot.Contract.Utility;
public static partial class StringUtil
{
	/// <summary>
	/// Turns repeating sequence of spaces into one
	/// </summary>
	public static string NormalizeSpaces(this string arg)
	{
		return SpacesDupsRemoveRegex().Replace(arg, " ");
	}

	[GeneratedRegex(@"\s+")]
	private static partial Regex SpacesDupsRemoveRegex();
}
