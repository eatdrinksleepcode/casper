using System;
using System.IO;
using System.Text;

namespace Casper {
	public abstract class RedirectedStandardOutput : IDisposable {
		private readonly TextWriter originalOutput;
		private readonly StringBuilder output = new StringBuilder();

		private RedirectedStandardOutput() {
			originalOutput = Get();
			Set(new StringWriter(output));
		}

		private RedirectedStandardOutput(Func<TextWriter, TextWriter> redirectTarget) {
			originalOutput = Get();
			Set(redirectTarget(originalOutput));
		}

		public void Clear() {
			output.Clear();
		}

		public override string ToString() {
			return output.ToString();
		}

		public void Dispose() {
			Set(originalOutput);
		}

		protected abstract TextWriter Get();

		protected abstract void Set(TextWriter writer);

		public static RedirectedStandardOutput RedirectOut() {
			return new Out();
		}

		public static IDisposable RedirectOut(Func<TextWriter, TextWriter> redirectTarget) {
			return new Out(redirectTarget);
		}

		public static RedirectedStandardOutput RedirectError() {
			return new Error();
		}

		public static IDisposable RedirectError(Func<TextWriter, TextWriter> redirectTarget) {
			return new Error(redirectTarget);
		}
			
		private class Out : RedirectedStandardOutput {

			public Out() {
			}

			public Out(Func<TextWriter, TextWriter> redirectTarget)
				: base(redirectTarget) {
			}

			protected override TextWriter Get() {
				return Console.Out;
			}

			protected override void Set(TextWriter writer) {
				Console.SetOut(writer);
			}
		}

		private class Error : RedirectedStandardOutput {

			public Error() {
			}

			public Error(Func<TextWriter, TextWriter> redirectTarget)
				: base(redirectTarget) {
			}

			protected override TextWriter Get() {
				return Console.Error;
			}

			protected override void Set(TextWriter writer) {
				Console.SetError(writer);
			}
		}
	}
}

