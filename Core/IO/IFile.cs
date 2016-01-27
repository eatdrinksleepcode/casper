using System;
using System.IO;

namespace Casper.IO {
	public interface IFile : IFileSystemObject {
		void CopyTo(IFile destination);

		void WriteAllText(string text);
		void WriteAll<T>(T content);

		TextReader OpenText();

		string ReadAllText();
		T ReadAll<T>();

		void CreateDirectories();

		IDirectory Directory { get; }

		DateTimeOffset LastWriteTimeUtc { get; }
	}
}
