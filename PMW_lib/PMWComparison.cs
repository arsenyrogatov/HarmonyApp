using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PMW_lib
{
    public static class PMWComparison
    {
        static ReaderWriterLockSlim ReaderWriterLock = new();

        public static async Task<SoundFingerprinting.Query.AVQueryResult?> CompareAVHashesAsync (AVHashes hashes)
        {
            SoundFingerprinting.Query.AVQueryResult? result = null;

            ReaderWriterLock.EnterReadLock();
            try
            {
                result = await QueryCommandBuilder.Instance
                                             .BuildQueryCommand()
                                             .From(hashes)
                                             .UsingServices(PMWFingerprinting.InMemoryModelService)
                                             .Query();
            }
            finally
            {
                // Ensure that the lock is released.
                ReaderWriterLock.ExitReadLock();
            }
            return result;
        }

        public static void StoreAVHashes(string path, AVHashes hashes)
        {
            ReaderWriterLock.EnterWriteLock();
            try
            {
                var TrackId = path;
                var track = new TrackInfo(TrackId, String.Empty, String.Empty);
                //modelService.Insert(track, hashes);
                PMWFingerprinting.InMemoryModelService.Insert(track, hashes);
            }
            finally { ReaderWriterLock.ExitWriteLock();}
        }
    }
}
