using System.Collections.Generic;
using System.IO;
using System.Text;
using Casper.IO;

namespace Casper.IO {

	public class StubFileSystem : IFileSystem {

		private readonly IDictionary<string, IFile> files = new Dictionary<string, IFile>();

		public class StubFile : IFile {
			private readonly string path;
			private MemoryStream content;

			public StubFile(string path) {
				this.path = path;
			}

			public bool Exists() {
				return null != content;
			}

			public void Delete() {
				content = null;
			}

			public string ReadAllText() {
				content.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(content, Encoding.UTF8, false, 1024, true)) {
					return reader.ReadToEnd();
				}
			}

			public void WriteAllText(string text) {
				var bytes = Encoding.UTF8.GetBytes(text);
				content = new MemoryStream(bytes, 0, bytes.Length, true, true);
			}

			public void CopyTo(IFile destination) {
				var stubDestination = destination as StubFile;
				var newBuffer = (byte[])content.GetBuffer().Clone();
				stubDestination.content = new MemoryStream(newBuffer);
			}

			public string Path {
				get { return path; }
			}
		}

		public IFile File(string path) {
			IFile file;
			if (!files.TryGetValue(path, out file)) {
				file = new StubFile(path);
				files.Add(path, file);
			}
			return file;
		}
	}
}
