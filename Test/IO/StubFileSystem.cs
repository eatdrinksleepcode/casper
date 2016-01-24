using System.Collections.Generic;
using System.IO;
using System.Text;
using Casper.IO;
using System;

namespace Casper.IO {

	public class StubFileSystem : IFileSystem {

		private readonly IDictionary<string, IFileSystemObject> files = new Dictionary<string, IFileSystemObject>();
		private IDirectory currentDirectory;

		public StubFileSystem() { 
			this.currentDirectory = Directory(System.IO.Path.DirectorySeparatorChar.ToString());
		}

		public class StubFile : IFile {
			private readonly string path;
			private DateTimeOffset lastWriteTimeUtc;
			private MemoryStream contentStream;

			public StubFile(string path) {
				this.path = path;
			}

			public bool Exists() {
				return null != contentStream;
			}

			public void Delete() {
				contentStream = null;
			}

			public string ReadAllText() {
				contentStream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(contentStream, Encoding.UTF8, false, 1024, true)) {
					return reader.ReadToEnd();
				}
			}

			public void WriteAllText(string text) {
				var bytes = Encoding.UTF8.GetBytes(text);
				contentStream = new MemoryStream(bytes, 0, bytes.Length, true, true);
			}

			public void CopyTo(IFile destination) {
				var stubDestination = destination as StubFile;
				var newBuffer = (byte[])contentStream.GetBuffer().Clone();
				stubDestination.contentStream = new MemoryStream(newBuffer);
			}

			public string Path {
				get { return path; }
			}
		}

		private class StubDirectory : IDirectory {
			private StubFileSystem fileSystem;
			private readonly string path;
			private bool exists = false;

			public StubDirectory(StubFileSystem fileSystem, string path) {
				this.fileSystem = fileSystem;
				this.path = path;
			}

			public IFile File(string relativePath) {
				return fileSystem.File(System.IO.Path.Combine(path, relativePath));
			}

			public void SetAsCurrent() {
				fileSystem.SetCurrentDirectory(this);
			}

			public bool Exists() {
				return exists;
			}

			public void Delete() {
				exists = false;
			}

			public string Path {
				get {
					return path;
				}
			}
		}

		public IFile File(string path) {
			IFileSystemObject fileSystemObject;
			IFile file;
			if (!files.TryGetValue(path, out fileSystemObject)) {
				file = new StubFile(path);
				files.Add(path, file);
			} else {
				file = fileSystemObject as StubFile;
				if (null == file) {
					if (fileSystemObject.Exists()) {
						throw new Exception(string.Format("'{0}' is not a file", path));
					} else {
						file = new StubFile(path);
						files[path] = file;
					}
				}
			}
			return file;
		}

		public IDirectory Directory(string path) {
			IFileSystemObject fileSystemObject;
			IDirectory directory;
			if (!files.TryGetValue(path, out fileSystemObject)) {
				directory = new StubDirectory(this, path);
				files.Add(path, directory);
			} else {
				directory = fileSystemObject as StubDirectory;
				if (null == directory) {
					if (fileSystemObject.Exists()) {
						throw new Exception(string.Format("'{0}' is not a directory", path));
					} else {
						directory = new StubDirectory(this, path);
						files[path] = directory;
					}
				}
			}
			return directory;
		}

		public IDirectory GetCurrentDirectory() {
			return this.currentDirectory;
		}

		public void SetCurrentDirectory(IDirectory directory) {
			this.currentDirectory = directory;
		}

	}
}
