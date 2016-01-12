namespace Casper.IO {
	public class RealFileSystem : IFileSystem {
		
		public IFile File(string path) {
			return new RealFile(path);
		}

		public class RealFile : IFile {
			private readonly string path;

			public RealFile(string path) {
				this.path = path;
			}

			public void WriteAllText(string text) {
				System.IO.File.WriteAllText(path, text);
			}

			public string ReadAllText() {
				return System.IO.File.ReadAllText(path);
			}

			public bool Exists() {
				return System.IO.File.Exists(path);
			}

			public void Delete() {
				System.IO.File.Delete(path);
			}

			public void CopyTo(IFile destination) {
				System.IO.File.Copy(path, destination.Path, true);
			}

			public string Path {
				get {
					return path;
				}
			}
		}
	}
}
