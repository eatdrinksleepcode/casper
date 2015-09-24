using System.IO;

namespace Casper {
	public static class PathExtensions {
		public static string Parent(this string path) {
			return Path.GetDirectoryName(path);
		}

		public static string SubDirectory(this string path, string directory) {
			return Path.Combine(path, directory);
		}

		public static string File(this string path, string directory) {
			return Path.Combine(path, directory);
		}

		public static bool Exists(this string path) {
			return System.IO.File.Exists(path);
		}
	}
}
