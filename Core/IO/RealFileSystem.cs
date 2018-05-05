using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Casper.IO {
	public class RealFileSystem : IFileSystem {

		public static readonly RealFileSystem Instance = new RealFileSystem();

		private RealFileSystem() { }
		
		public IFile File(string path) {
			return new RealFile(path);
		}

		public IDirectory Directory(string path) {
			return new RealDirectory(path);
		}

		public IDirectory GetCurrentDirectory() {
			return Directory(System.IO.Directory.GetCurrentDirectory());
		}

		public IDirectory MakeTemporaryDirectory() {
			var tempFolder = Path.GetTempFileName();
			System.IO.File.Delete(tempFolder);
			var result = Directory(tempFolder);
			result.Create();

			return result;
		}

		private class RealFile : IFile {
			public RealFile(string path) {
				FullPath = System.IO.Path.IsPathRooted(path) 
					? path 
					: System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
			}

			public void WriteAllText(string text) {
				System.IO.File.WriteAllText(FullPath, text);
			}

			public void WriteAll<T>(T content) {
				var formatter = new BinaryFormatter();
				using (var stream = System.IO.File.OpenWrite(FullPath)) {
					formatter.Serialize(stream, content);
				}
			}

			public string ReadAllText() {
				return System.IO.File.ReadAllText(FullPath);
			}

			public IEnumerable<string> ReadAllLines() {
				return System.IO.File.ReadAllLines(FullPath);
			}

			public T ReadAll<T>() {
				var formatter = new BinaryFormatter();
				using (var stream = System.IO.File.OpenRead(FullPath)) {
					return (T)formatter.Deserialize(stream);
				}
			}

			public bool Exists() {
				return System.IO.File.Exists(FullPath);
			}

			public void Delete() {
				System.IO.File.Delete(FullPath);
			}

			public void CopyTo(IFile destination) {
				System.IO.File.Copy(FullPath, destination.FullPath, true);
			}

			public void CreateDirectories() {
				System.IO.Directory.CreateDirectory(DirectoryPath);
			}

			public TextReader OpenText() {
				return System.IO.File.OpenText(FullPath);
			}

			private string DirectoryPath => System.IO.Path.GetDirectoryName(FullPath);

			public DateTimeOffset LastWriteTimeUtc => System.IO.File.GetLastWriteTimeUtc(FullPath);

			public string FullPath { get; }

			public IDirectory Directory => new RealDirectory(DirectoryPath);

			public string Name => System.IO.Path.GetFileName(FullPath);
		}

		private class RealDirectory : IDirectory {
			public RealDirectory(string path) {
				FullPath = System.IO.Path.IsPathRooted(path) 
					? path 
					: System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
			}

			public IFile File(string relativePath) {
				return new RealFile(System.IO.Path.Combine(FullPath, relativePath));
			}

			public IDirectory Directory (string relativePath) {
				return new RealDirectory(System.IO.Path.Combine(FullPath, relativePath));
			}

			public void SetAsCurrent() {
				System.IO.Directory.SetCurrentDirectory(FullPath);
			}

			public bool Exists() {
				return System.IO.Directory.Exists(FullPath);
			}

			public void Delete() {
				try {
					System.IO.Directory.Delete(FullPath, true);
				} catch(DirectoryNotFoundException) {
					// Desired result is for directory to not exist, which is true
					// Consider this successful
				}
			}

			public void Create() {
				System.IO.Directory.CreateDirectory(FullPath);
			}

			public string FullPath { get; }

			public IDirectory RootDirectory => Directory(System.IO.Directory.GetDirectoryRoot(FullPath));

			public string Name => System.IO.Path.GetFileName(FullPath);
		}
	}
}
