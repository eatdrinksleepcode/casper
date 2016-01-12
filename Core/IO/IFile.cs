namespace Casper.IO {
	public interface IFile {
		bool Exists();
		void Delete();

		void CopyTo(IFile destination);

		void WriteAllText(string text);
		string ReadAllText();

		string Path {
			get;
		}
	}
}
