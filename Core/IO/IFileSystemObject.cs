
namespace Casper.IO
{
	public interface IFileSystemObject
	{
		bool Exists();
		void Delete();

		string Path {
			get;
		}
	}

}
