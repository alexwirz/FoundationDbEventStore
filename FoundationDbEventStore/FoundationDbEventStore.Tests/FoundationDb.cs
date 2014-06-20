using FoundationDB.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoundationDbEventStore.Tests
{
    class FoundationDb
    {
        public static void RemoveDirectory (IEnumerable<string> path)
        {
            var cancellationToken = new CancellationToken();
            RemoveDirectoryAsync (path, cancellationToken).Wait ();
        }

        public static async Task RemoveDirectoryAsync (IEnumerable<string> path, CancellationToken cancellationToken)
        {
            using (var db = await Fdb.OpenAsync())
            {
                await db.Directory.TryRemoveAsync(path, cancellationToken);
            }
        }
    }
}
