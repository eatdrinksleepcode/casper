
namespace Casper.IO
{
	public interface IFileSystemObject
	{
		bool Exists();
		void Delete();

		string FullPath {
			get;
		}
	}

}
