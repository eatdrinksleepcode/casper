namespace Casper.IO {
	public interface IDirectory : IFileSystemObject {
		IFile File(string relativePath);
		void SetAsCurrent();
	}
}
