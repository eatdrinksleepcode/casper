using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Casper.IO {

	public class StubFileSystem : IFileSystem {

		private readonly IDictionary<string, IFileSystemObject> files = new Dictionary<string, IFileSystemObject>();
		private IDirectory currentDirectory;

		public StubFileSystem() { 
			this.currentDirectory = Directory(System.IO.Path.DirectorySeparatorChar.ToString());
		}

		public class StubFile : IFile {
			private readonly StubFileSystem fileSystem;
			private readonly string path;
			private DateTimeOffset lastWriteTimeUtc;
			private MemoryStream contentStream;

			public StubFile(StubFileSystem fileSystem, string path) {
				this.fileSystem = fileSystem;
				this.path = path;
			}

			public bool Exists() {
				return null != contentStream;
			}

			public void Delete() {
				lastWriteTimeUtc = DateTimeOffset.MinValue;
				contentStream = null;
			}

			public string ReadAllText() {
				contentStream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(contentStream, Encoding.UTF8, false, 1024, true)) {
					return reader.ReadToEnd();
				}
			}

			public IEnumerable<string> ReadAllLines() {
				contentStream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(contentStream, Encoding.UTF8, false, 1024, true)) {
					string line;
					while((line = reader.ReadLine()) != null) {
						yield return line;
					}
				}
			}

			public T ReadAll<T>() {
				BinaryFormatter formatter = new BinaryFormatter();
				contentStream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(contentStream);
			}

			public void WriteAllText(string text) {
				var bytes = Encoding.UTF8.GetBytes(text);
				WriteContent(bytes);
			}

			public void WriteAll<T>(T content) {
				BinaryFormatter formatter = new BinaryFormatter();
				var newContentStream = new MemoryStream();
				formatter.Serialize(newContentStream, content);
				WriteContent(newContentStream.ToArray());
			}

			private void WriteContent(byte[] newContent) {
				this.contentStream = new MemoryStream(newContent, 0, newContent.Length, true, true);
				lastWriteTimeUtc = DateTimeOffset.UtcNow;
			}

			public void CopyTo(IFile destination) {
				var stubDestination = (StubFile)destination;
				stubDestination.WriteContent((byte[])contentStream.GetBuffer().Clone());
			}

			public void CreateDirectories() {
			}

			public TextReader OpenText() {
				contentStream.Seek(0, SeekOrigin.Begin);
				return new StreamReader(contentStream);
			}

			public DateTimeOffset LastWriteTimeUtc {
				get { return lastWriteTimeUtc; }
			}

			public string FullPath {
				get { return path; }
			}

			public IDirectory Directory {
				get { return fileSystem.Directory(System.IO.Path.GetDirectoryName(path)); }
			}

			public string Name {
				get {
					return System.IO.Path.GetFileName(path);
				}
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

			public IDirectory Directory(string relativePath) {
				return fileSystem.Directory(System.IO.Path.Combine(path, relativePath));
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

			public void Create() {
				exists = true;
			}

			public string FullPath {
				get { return path; }
			}

			public IDirectory RootDirectory {
				get { return new StubDirectory(fileSystem, System.IO.Path.GetPathRoot(path)); }
			}

			public string Name {
				get {
					return System.IO.Path.GetDirectoryName(System.IO.Path.Combine(path, "a"));
				}
			}
		}

		public IFile File(string path) {
			IFileSystemObject fileSystemObject;
			IFile file;
			path = System.IO.Path.IsPathRooted(path) 
				? path 
				: System.IO.Path.Combine(GetCurrentDirectory().FullPath, path);
			if (!files.TryGetValue(path, out fileSystemObject)) {
				file = new StubFile(this, path);
				files.Add(path, file);
			} else {
				file = fileSystemObject as StubFile;
				if (null == file) {
					if (fileSystemObject.Exists()) {
						throw new Exception(string.Format("'{0}' is not a file", path));
					} else {
						file = new StubFile(this, path);
						files[path] = file;
					}
				}
			}
			return file;
		}

		public IDirectory Directory(string path) {
			IFileSystemObject fileSystemObject;
			IDirectory directory;
			path = System.IO.Path.IsPathRooted(path) 
			             ? path 
			             : System.IO.Path.Combine(GetCurrentDirectory().FullPath, path);
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

		public IDirectory MakeTemporaryDirectory() {
			var result = Directory(Path.GetRandomFileName());
			result.Create();
			return result;
		}
	}
}
