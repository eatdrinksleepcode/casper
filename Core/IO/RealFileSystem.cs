using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Casper.IO {
	public class RealFileSystem : IFileSystem {
		
		public IFile File(string path) {
			return new RealFile(path);
		}

		public IDirectory Directory(string path) {
			return new RealDirectory(path);
		}

		public IDirectory GetCurrentDirectory() {
			return Directory(System.IO.Directory.GetCurrentDirectory());
		}

		private class RealFile : IFile {
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

		private class RealDirectory : IDirectory {
			private readonly string path;

			public RealDirectory(string path) {
				this.path = path;
			}

			public IFile File(string relativePath) {
				return new RealFile(System.IO.Path.Combine(path, relativePath));
			}

			public void SetAsCurrent() {
				System.IO.Directory.SetCurrentDirectory(path);
			}

			public bool Exists() {
				return System.IO.Directory.Exists(path);
			}

			public void Delete() {
				System.IO.Directory.Delete(path, true);
			}

			public string Path {
				get {
					return path;
				}
			}
		}
	}
}
