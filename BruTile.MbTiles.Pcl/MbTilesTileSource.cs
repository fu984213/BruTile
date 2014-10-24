﻿// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

// This file was created by Felix Obermaier (www.ivv-aachen.de) 2011.

using System;
using BruTile.Cache;
using BruTile.FileSystem;
using BruTile.Predefined;
using SQLite.Net;
using SQLite.Net.Interop;

namespace BruTile
{
    /// <summary>
    /// An <see cref="ITileSource"/> implementation for MapBox Tiles files
    /// </summary>
    /// <seealso href="https://www.mapbox.com/developers/mbtiles/"/>
    public class MbTilesTileSource : ITileSource //, System.Runtime.Serialization.ISerializable
    {
        private static ISQLitePlatform _platform;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="connectionString">The MapBox tiles file</param>
        /// <param name="schema">The tile schema (should be of <see cref="GlobalMercator"/></param>
        /// <param name="type">The type of the MapBox tiles file</param>
        public MbTilesTileSource(SQLiteConnectionString connectionString, ITileSchema schema = null, MbTilesType type = MbTilesType.None)
            : this(new MbTilesProvider(connectionString, schema, type))
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="provider">The MapBox Tiles provider</param>
        internal MbTilesTileSource(MbTilesProvider provider)
        {
            _tileProvider = provider;
        }
        private readonly MbTilesProvider _tileProvider;

        #region Implementation of ITileSource

        /// <summary>
        /// Gets a value indicating the tile provider
        /// </summary>
        public ITileProvider Provider
        {
            get { return _tileProvider; }
        }

        /// <summary>
        /// Gets a value indicating the schema of the tile source
        /// </summary>
        public ITileSchema Schema
        {
            get { return _tileProvider.Schema; }
        }

        /// <summary>
        /// Gets a value indicating the name of the tile source
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating the (image-)format of the tiles
        /// </summary>
        public MbTilesFormat Format
        {
            get { return _tileProvider.Cache.Format; }
        }

        /// <summary>
        /// Gets a value indicating the type of the tiles
        /// </summary>
        public MbTilesType Type { get { return _tileProvider.Cache.Type; } }

        /// <summary>
        /// Method to initialize SQLite.Net with the platform it is used with.
        /// </summary>
        /// <param name="platform"></param>
        public static void SetPlatform(ISQLitePlatform platform)
        {
            if (platform == null)
                throw new ArgumentNullException("platform");

            MbTilesCache.SetConnectionPool(new SQLiteConnectionPool(platform));
        }

        /// <summary>
        /// Gets a value indicating the covered extent
        /// </summary>
        public Extent Extent { get { return _tileProvider.Cache.Extent; }}

        #endregion Implementation of ITileSource
    }
}