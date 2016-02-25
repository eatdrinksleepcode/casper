namespace Casper {
	public static class Environment {
		public static bool IsUnix {
			get { return System.Environment.OSVersion.Platform == System.PlatformID.Unix; }
		}

		public static string NewLine {
			get { return System.Environment.NewLine; }
		}
	}
}
