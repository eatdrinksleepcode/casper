﻿using System;

namespace Casper {

	public class CasperException : Exception {

		public enum KnownExitCode : byte {
			None = 0,
			CompilationError = 1,
			MissingTask = 2,
			ConfigurationError = 3,
			TaskFailed = 4,
			InvocationError = 5,
			UnhandledException = 255,
		}

		public CasperException(KnownExitCode exitCode, string message)
			: base(message) {
			ExitCode = exitCode;
		}

		public CasperException(KnownExitCode exitCode, Exception innerException)
			: base(innerException.Message, innerException) {
			ExitCode = exitCode;
		}

		public KnownExitCode ExitCode { get; }
	}
}
