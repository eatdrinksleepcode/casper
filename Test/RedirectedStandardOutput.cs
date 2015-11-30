using System;
using System.IO;
using System.Text;

namespace Casper {
	public abstract class RedirectedStandardOutput : IDisposable {
		private readonly TextWriter originalOutput;
		private readonly StringBuilder output = new StringBuilder();

		private RedirectedStandardOutput() {
			this.originalOutput = Get();
			Set(new StringWriter(output));
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

		public static RedirectedStandardOutput RedirectError() {
			return new Error();
		}
			
		private class Out : RedirectedStandardOutput {
			protected override TextWriter Get() {
				return Console.Out;
			}

			protected override void Set(TextWriter writer) {
				Console.SetOut(writer);
			}
		}

		private class Error : RedirectedStandardOutput {
			protected override TextWriter Get() {
				return Console.Error;
			}

			protected override void Set(TextWriter writer) {
				Console.SetError(writer);
			}
		}
	}
}

