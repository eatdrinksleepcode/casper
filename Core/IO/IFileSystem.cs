namespace Casper.IO {
	public interface IFileSystem {
		IFile File(string path);
		IDirectory Directory(string path);
		IDirectory GetCurrentDirectory();
	}
}
