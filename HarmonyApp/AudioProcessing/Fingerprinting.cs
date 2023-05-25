﻿using SoundFingerprinting.Audio.NAudio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Extensions.LMDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyApp.AudioProcessing
{
    public class Fingerprinting
    {
        private static LMDBModelService? modelService;
        //public static InMemoryModelService InMemoryModelService = new InMemoryModelService();
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
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<SoundFingerprinting.Query.AVQueryResult?> CompareAVHashesAsync(AVHashes hashes)
        {

            return await QueryCommandBuilder.Instance
                                             .BuildQueryCommand()
                                             .From(hashes)
                                             .UsingServices(modelService)
                                             .Query();
        }

        public static void StoreAVHashes(string path, AVHashes hashes)
        {
            //Console.WriteLine(InMemoryModelService.GetTrackIds().Count().ToString());
            var TrackId = path;
            var track = new TrackInfo(TrackId, String.Empty, String.Empty);
            //modelService.Insert(track, hashes);
            modelService.Insert(track, hashes);
        }

        public static void DisposeModelService()
        {
            modelService.Dispose();

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