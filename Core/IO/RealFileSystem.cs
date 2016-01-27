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

		public class RealFile : IFile {
			private readonly string path;

			public RealFile(string path) {
				this.path = System.IO.Path.IsPathRooted(path) 
					? path 
					: System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
			}

			public void WriteAllText(string text) {
				System.IO.File.WriteAllText(path, text);
			}

			public void WriteAll<T>(T content) {
				var formatter = new BinaryFormatter();
				using (var stream = System.IO.File.OpenWrite(path)) {
					formatter.Serialize(stream, content);
				}
			}

			public string ReadAllText() {
				return System.IO.File.ReadAllText(path);
			}

			public T ReadAll<T>() {
				var formatter = new BinaryFormatter();
				using (var stream = System.IO.File.OpenRead(path)) {
					return (T)formatter.Deserialize(stream);
				}
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

			public void CreateDirectories() {
				System.IO.Directory.CreateDirectory(DirectoryPath);
			}

			public TextReader OpenText() {
				return System.IO.File.OpenText(path);
			}

			private string DirectoryPath {
				get { return System.IO.Path.GetDirectoryName(path); }
			}

			public DateTimeOffset LastWriteTimeUtc {
				get { return System.IO.File.GetLastWriteTimeUtc(path); }
			}

			public string Path {
				get { return path; }
			}

			public IDirectory Directory {
				get { return new RealDirectory(DirectoryPath); }
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

			public IDirectory Directory (string relativePath) {
				return new RealDirectory(relativePath);
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
