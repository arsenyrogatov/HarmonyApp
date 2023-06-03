using SoundFingerprinting.Audio.NAudio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Extensions.LMDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace HarmonyApp.AudioProcessing
{
    public class Fingerprinting
    {
        private static LMDBModelService? modelService;
        //private static InMemoryModelService service = new();
        private static string? DbPath;
        private static readonly NAudioService audioService = new();

        public static void InitilizeModelService()
        {
            var tempDbPath = Path.GetTempPath() + @"HarmonyDuplicateFinder\Database";
            try
            {
                if (Directory.Exists(tempDbPath))
                    Directory.Delete(tempDbPath, true);
            }
            catch
            {
                Random random = new();
                tempDbPath += @"\DB" + random.Next(int.MaxValue);
            }
            DbPath = tempDbPath;
            modelService = new LMDBModelService(DbPath);
        }

        public static async Task<AVHashes> GetAVHashesAsync(string path)
        {
            try
            {
                return await FingerprintCommandBuilder.Instance
                                                           .BuildFingerprintCommand()
                                                           .From(path)
                                                           .WithFingerprintConfig(config =>
                                                           {
                                                               // audio configuration
                                                               config.Audio.SampleRate = 5512;
                                                               return config;
                                                           })
                                                           .UsingServices(audioService)
                                                           .Hash();
            }
            catch
            {
                return AVHashes.Empty;
            }
        }

        public static async Task<SoundFingerprinting.Query.AVQueryResult?> CompareAVHashesAsync(AVHashes hashes)
        {
            if (hashes.IsEmpty)
                return null;
            else
            {
                SoundFingerprinting.Query.AVQueryResult? result = null;
                try
                {
                    if (modelService is not null)
                        result = await QueryCommandBuilder.Instance
                                                         .BuildQueryCommand()
                                                         .From(hashes)
                                                         .UsingServices(modelService)
                                                         //.UsingServices(service)
                                                         .Query();
                }
                catch
                {
                    result = null;
                }
                return result;
            }
        }

        public static void StoreAVHashes(string path, AVHashes hashes)
        {
            var TrackId = path;
            var track = new TrackInfo(TrackId, String.Empty, String.Empty);
            if (modelService is not null)
                try
                {
                    modelService.Insert(track, hashes);
                }
                catch
                {

                }
            //service.Insert(track, hashes);
        }

        public static void DisposeModelService()
        {
            modelService?.Dispose();

            try
            {
                if (Directory.Exists(DbPath))
                    Directory.Delete(DbPath, true);
            }
            catch
            {

            }
        }
    }
}
