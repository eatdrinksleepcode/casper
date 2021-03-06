﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Casper.IO {

	public class StubFileSystem : IFileSystem {

		private readonly IDictionary<string, IFileSystemObject> files = new Dictionary<string, IFileSystemObject>();
		private IDirectory currentDirectory;

		public StubFileSystem() { 
			currentDirectory = Directory(System.IO.Path.DirectorySeparatorChar.ToString());
		}

		private class StubFile : IFile {
			private readonly StubFileSystem fileSystem;
			private MemoryStream contentStream;

			public StubFile(StubFileSystem fileSystem, string path) {
				this.fileSystem = fileSystem;
				this.FullPath = path;
			}

			public bool Exists() {
				return null != contentStream;
			}

			public void Delete() {
				LastWriteTimeUtc = DateTimeOffset.MinValue;
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
				var formatter = new BinaryFormatter();
				contentStream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(contentStream);
			}

			public void WriteAllText(string text) {
				var bytes = Encoding.UTF8.GetBytes(text);
				WriteContent(bytes);
			}

			public void WriteAll<T>(T content) {
				var formatter = new BinaryFormatter();
				var newContentStream = new MemoryStream();
				formatter.Serialize(newContentStream, content);
				WriteContent(newContentStream.ToArray());
			}

			private void WriteContent(byte[] newContent) {
				contentStream = new MemoryStream(newContent, 0, newContent.Length, true, true);
				LastWriteTimeUtc = DateTimeOffset.UtcNow;
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

			public DateTimeOffset LastWriteTimeUtc { get; private set; }

			public string FullPath { get; }

			public IDirectory Directory => fileSystem.Directory(System.IO.Path.GetDirectoryName(FullPath));

			public string Name => System.IO.Path.GetFileName(FullPath);
		}

		private class StubDirectory : IDirectory {
			private readonly StubFileSystem fileSystem;
			private bool exists;

			public StubDirectory(StubFileSystem fileSystem, string path) {
				this.fileSystem = fileSystem;
				this.FullPath = path;
			}

			public IFile File(string relativePath) {
				return fileSystem.File(System.IO.Path.Combine(FullPath, relativePath));
			}

			public IDirectory Directory(string relativePath) {
				return fileSystem.Directory(System.IO.Path.Combine(FullPath, relativePath));
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

			public string FullPath { get; }

			public IDirectory RootDirectory => new StubDirectory(fileSystem, System.IO.Path.GetPathRoot(FullPath));

			public string Name => System.IO.Path.GetFileName(FullPath);
		}

		public IFile File(string path) {
			IFile file;
			path = System.IO.Path.IsPathRooted(path) 
				? path 
				: System.IO.Path.Combine(GetCurrentDirectory().FullPath, path);
			path = System.IO.Path.GetFullPath(path);
			if (!files.TryGetValue(path, out var fileSystemObject)) {
				file = new StubFile(this, path);
				files.Add(path, file);
			} else {
				file = fileSystemObject as StubFile;
				if (null == file) {
					if (fileSystemObject.Exists()) {
						throw new Exception($"'{path}' is not a file");
					} else {
						file = new StubFile(this, path);
						files[path] = file;
					}
				}
			}
			return file;
		}

		public IDirectory Directory(string path) {
			IDirectory directory;
			path = System.IO.Path.IsPathRooted(path) 
			             ? path 
			             : System.IO.Path.Combine(GetCurrentDirectory().FullPath, path);
			path = System.IO.Path.GetFullPath(path);
			if (!files.TryGetValue(path, out var fileSystemObject)) {
				directory = new StubDirectory(this, path);
				files.Add(path, directory);
			} else {
				directory = fileSystemObject as StubDirectory;
				if (null == directory) {
					if (fileSystemObject.Exists()) {
						throw new Exception($"'{path}' is not a directory");
					} else {
						directory = new StubDirectory(this, path);
						files[path] = directory;
					}
				}
			}
			return directory;
		}

		public IDirectory GetCurrentDirectory() {
			return currentDirectory;
		}

		private void SetCurrentDirectory(IDirectory directory) {
			currentDirectory = directory;
		}

		public IDirectory MakeTemporaryDirectory() {
			var result = Directory(Path.GetRandomFileName());
			result.Create();
			return result;
		}
	}
}
