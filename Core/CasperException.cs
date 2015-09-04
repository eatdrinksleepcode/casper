using System;

namespace Casper {

	public class CasperException : Exception {
		public const int EXIT_CODE_COMPILATION_ERROR = 1;
		public const int EXIT_CODE_MISSING_TASK = 2;
		public const int EXIT_CODE_CONFIGURATION_ERROR = 3;
		public const int EXIT_CODE_TASK_FAILED = 4;
		public const int EXIT_CODE_UNHANDLED_EXCEPTION = 255;

		private readonly int exitCode;

		public CasperException(int exitCode, string message, params object[] args)
			: base(string.Format(message, args)) {
			this.exitCode = exitCode;
		}

		public int ExitCode {
			get {
				return exitCode;
			}
		}
	}
}

