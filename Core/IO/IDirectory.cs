namespace Casper.IO {
	public interface IDirectory : IFileSystemObject {
		IFile File(string relativePath);
		IDirectory Directory(string relativePath);
		void SetAsCurrent();
		void Create();

		IDirectory RootDirectory { get; }
	}
}
