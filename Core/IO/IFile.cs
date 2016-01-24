namespace Casper.IO {
	public interface IFile : IFileSystemObject {
		void CopyTo(IFile destination);

		void WriteAllText(string text);
		string ReadAllText();
	}
}
