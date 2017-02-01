namespace Casper {
	public static class Environment {
		public static bool IsUnix {
			get { return System.Environment.OSVersion.Platform == System.PlatformID.Unix; }
		}

		public static bool IsMono {
			get { return null != System.Type.GetType("Mono.Runtime"); }
		}

		// HACK: there should probably be a better way to do this
		public static bool IsUnixFileSystem {
			get { return System.IO.Path.DirectorySeparatorChar == '/'; }
		}

		public static string NewLine {
			get { return System.Environment.NewLine; }
		}
	}
}
