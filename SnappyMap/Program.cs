﻿namespace SnappyMap
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Security.Cryptography;
    using System.Xml.Serialization;

    using CommandLine;

    using SnappyMap.IO;

    public class Program
    {
        private const int ErrorExitCode = 1;
        private const int SuccessExitCode = 0;

        public static int Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
            {
                return 1;
            }

            if (options.Items.Count < 1)
            {
                Console.WriteLine("Missing required arguments.");
                Console.WriteLine();
                Console.WriteLine(options.GetUsage());
                return ErrorExitCode;
            }

            string[] parts = options.Size.Split(new[] { 'x' }, 2);
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid map size: " + options.Size);
                Console.WriteLine();
                Console.WriteLine(options.GetUsage());
                return ErrorExitCode;
            }

            int mapWidth;
            int mapHeight;

            if (!int.TryParse(parts[0], out mapWidth))
            {
                Console.WriteLine("Invalid map width: " + parts[0]);
                Console.WriteLine();
                Console.WriteLine(options.GetUsage());
                return ErrorExitCode;
            }

            if (!int.TryParse(parts[1], out mapHeight))
            {
                Console.WriteLine("Invalid map height: " + parts[1]);
                Console.WriteLine();
                Console.WriteLine(options.GetUsage());
                return ErrorExitCode;
            }

            if (mapWidth <= 0 || mapHeight <= 0)
            {
                Console.WriteLine("Map dimensions ({0}x{1}) too small.", mapWidth, mapHeight);
                Console.WriteLine();
                Console.WriteLine(options.GetUsage());
                return ErrorExitCode;
            }

            // We only place tiles on grid intersections,
            // so maps are one tile shorter than they ought to be.
            // We tweak the size to compensate for that.
            mapWidth += 1;
            mapHeight += 1;

            string inputPath = options.Items[0];
            string outputPath = options.Items[1];
            string searchPath = options.LibraryPath;

            if (!File.Exists(options.ConfigFile))
            {
                Console.WriteLine("Config file not found: \"{0}\"", options.ConfigFile);
                return ErrorExitCode;
            }

            var configSerializer = new XmlSerializer(typeof(SectionConfig));
            SectionConfig config;
            try
            {
                using (Stream s = File.OpenRead(options.ConfigFile))
                {
                    config = (SectionConfig)configSerializer.Deserialize(s);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read config file: " + e.Message);
                return ErrorExitCode;
            }

            ITerrainCreator creator;
            try
            {
                creator = CreateFuzzyTerrainCreator(searchPath, config, mapWidth, mapHeight);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading section library: " + e.Message);
                return ErrorExitCode;
            }

            ISectionSerializer serializer = new SectionSerializer();

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Input file '{0}' not found.", inputPath);
                return ErrorExitCode;
            }

            Bitmap image = new Bitmap(inputPath);

            Section terrain = creator.CreateTerrainFrom(image);

            try
            {
                using (Stream output = File.Create(outputPath))
                {
                    serializer.WriteSection(output, terrain);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing output: " + e.Message);
                return ErrorExitCode;
            }

            return SuccessExitCode;
        }

        private static ITerrainCreator CreateTerrainCreator(
            string tilesetDirectory,
            SectionConfig config,
            int mapWidth,
            int mapHeight)
        {
            var tileDatabase = new TileDatabase(SHA1.Create());

            var sectionDatabaseFactory = new SectionDatabaseFactory(new SectionLoader(tileDatabase));
            var sectionDatabase = sectionDatabaseFactory.CreateDatabaseFrom(tilesetDirectory, config);

            var sectionDecider = new SectionDecider(
                new SectionTypeLabeler(),
                new SectionTypeRealizer(sectionDatabase));

            return new TerrainCreator(
                new MapQuantizer(mapWidth, mapHeight),
                sectionDecider,
                new SectionGridRenderer());
        }

        private static ITerrainCreator CreateFuzzyTerrainCreator(
            string tilesetDirectory,
            SectionConfig config,
            int mapWidth,
            int mapHeight)
        {
            var tileDatabase = new TileDatabase(SHA1.Create());

            var sectionDatabaseFactory = new SectionDatabaseFactory(new SectionLoader(tileDatabase));
            var db = sectionDatabaseFactory.CreateFuzzyDatabaseFrom(tilesetDirectory, config);

            return new FuzzyTerrainCreator(
                new MapQuantizer(mapWidth, mapHeight),
                db,
                new SectionGridRenderer());
        }
    }
}
