using System;

namespace Casper {

	public class CasperException : Exception {

		public enum KnownExitCode : byte {
			None = 0,
			CompilationError = 1,
			MissingTask = 2,
			ConfigurationError = 3,
			TaskFailed = 4,
			InvocationgError = 5,
			UnhandledException = 255,
		}

		public CasperException(KnownExitCode exitCode, string message)
			: base(message) {
			ExitCode = exitCode;
		}

		public CasperException(KnownExitCode exitCode, string message, params object[] args)
			: this(exitCode, string.Format(message, args)) {
		}

		public CasperException(KnownExitCode exitCode, Exception innerException, string message)
			: base(message, innerException) {
			ExitCode = exitCode;
		}

		public CasperException(KnownExitCode exitCode, Exception innerException)
			: base(innerException.Message, innerException) {
			ExitCode = exitCode;
		}

		public KnownExitCode ExitCode { get; }
	}
}
