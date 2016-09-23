using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AirBreather.IO
{
    public static class AsyncFile
    {
        // see also:
        // https://github.com/dotnet/coreclr/blob/f5d7e048cf7d66c6dc8473d428bf2d82dddd07d5/src/mscorlib/src/System/IO/FileStream.cs#L378
        public const int PrimaryBufferSize = 4096;

        // largest multiple of 4096 that won't go to the LOH.  see also:
        // https://github.com/dotnet/coreclr/blob/f5d7e048cf7d66c6dc8473d428bf2d82dddd07d5/src/mscorlib/src/System/IO/Stream.cs#L41-L44
        public const int FullCopyBufferSize = 81920;

        public static Task CopyIfMissingAsync(string sourcePath, string destinationPath) => CopyIfMissingAsync(sourcePath, destinationPath, CancellationToken.None);

        public static Task CopyIfMissingAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken) => File.Exists(destinationPath)
            ? Task.CompletedTask
            : CopyAsync(sourcePath, destinationPath, cancellationToken);

        public static Task CopyAsync(string sourcePath, string destinationPath) => CopyAsync(sourcePath, destinationPath, CancellationToken.None);

        public static async Task CopyAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
        {
            using (FileStream sourceStream = OpenReadSequential(sourcePath))
            using (FileStream destinationStream = CreateSequential(destinationPath))
            {
                await sourceStream.CopyToAsync(destinationStream, FullCopyBufferSize, cancellationToken).ConfigureAwait(false);
            }
        }

        public static FileStream OpenRead(string path) => OpenRead(path, extraOptions: FileOptions.None);

        public static FileStream OpenReadSequential(string path) => OpenRead(path, extraOptions: FileOptions.SequentialScan);

        public static FileStream OpenRead(string path, FileOptions extraOptions) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, PrimaryBufferSize, FileOptions.Asynchronous | extraOptions);

        public static FileStream Create(string path) => Create(path, extraOptions: FileOptions.None);

        public static FileStream CreateSequential(string path) => Create(path, extraOptions: FileOptions.SequentialScan);

        public static FileStream Create(string path, FileOptions extraOptions) => new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, PrimaryBufferSize, FileOptions.Asynchronous | extraOptions);
    }
}
