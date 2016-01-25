using System;

namespace Casper.IO {
	public interface IFile : IFileSystemObject {
		void CopyTo(IFile destination);

		void WriteAllText(string text);
		void WriteAll<T>(T content);

		string ReadAllText();
		T ReadAll<T>();

		void CreateDirectories();

		DateTimeOffset LastWriteTimeUtc {
			get;
		}
	}
}
