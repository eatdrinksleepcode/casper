
namespace Casper.IO
{
	public interface IFileSystemObject
	{
		string Name { get; }
		bool Exists();
		void Delete();

		string FullPath {
			get;
		}
	}

}
