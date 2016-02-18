using System;
using System.Collections.Generic;
using System.IO;

namespace Casper.IO {
	public interface IFile : IFileSystemObject {
		void CopyTo(IFile destination);

		void WriteAll<T>(T content);
		void WriteAllText(string text);

		TextReader OpenText();

		T ReadAll<T>();
		string ReadAllText();
		IEnumerable<string> ReadAllLines();

		void CreateDirectories();

		IDirectory Directory { get; }

		DateTimeOffset LastWriteTimeUtc { get; }
	}
}
