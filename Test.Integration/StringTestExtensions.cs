namespace Casper {
	static class StringTestExtensions {
		public static string NormalizeNewLines(this string text) {
			return System.Text.RegularExpressions.Regex.Replace(text, "\r?\n", Environment.NewLine);
		}
	}
}
