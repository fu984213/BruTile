﻿// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BruTile.Cache
{

    public class MemoryCache<T> : ITileCache<T>, INotifyPropertyChanged
    {
        //for future implemenations or replacements of this class look 
        //into .net 4.0 System.Collections.Concurrent namespace.
        #region Fields

        private Dictionary<TileIndex, T> bitmaps
          = new Dictionary<TileIndex, T>();

        private Dictionary<TileIndex, DateTime> touched
          = new Dictionary<TileIndex, DateTime>();

        private object syncRoot = new object();
        private int maxTiles ;
        private int minTiles;

        #endregion

        #region Properties

        public int TileCount
        {
            get
            {
                return this.bitmaps.Count;
            }
        }

        #endregion 

        #region Public Methods

        public MemoryCache(int minTiles, int maxTiles)
        {
            if (minTiles >= maxTiles) throw new ArgumentException("minTiles should be smaller than maxTiles");
            if (minTiles < 0) throw new ArgumentException("minTiles should be larger than zero");
            if (maxTiles < 0) throw new ArgumentException("maxTiles should be larger than zero");

            this.minTiles = minTiles;
            this.maxTiles = maxTiles;
        }

        public void Add(TileIndex key, T item)
        {
            lock (syncRoot)
            {
                if (bitmaps.ContainsKey(key))
                {
                    bitmaps[key] = item;
                    touched[key] = DateTime.Now;
                }
                else
                {
                    touched.Add(key, DateTime.Now);
                    bitmaps.Add(key, item);
                    if (bitmaps.Count > maxTiles) CleanUp();
                    this.OnNotifyPropertyChange("TileCount");
                }
            }
        }

        public void Remove(TileIndex key)
        {
            lock (syncRoot)
            {
                if (!bitmaps.ContainsKey(key)) return; //ignore if not exists
                touched.Remove(key);
                bitmaps.Remove(key);
                this.OnNotifyPropertyChange("TileCount");
            }
        }

        public T Find(TileIndex key)
        {
            lock (syncRoot)
            {
                if (!bitmaps.ContainsKey(key))
                {
                    return default(T);
                }
                else
                {
                    touched[key] = DateTime.Now;
                    return bitmaps[key];
                }
            }
        }

        #endregion

        #region Private Methods

        private void CleanUp()
        {
            lock (syncRoot)
            {
                //Purpose: Remove the older tiles so that the newest x tiles are left.
                TouchPermaCache(touched);
                DateTime cutoff = GetCutOff(touched, minTiles);
                List<TileIndex> oldItems = GetOldItems(touched, ref cutoff);
                foreach (TileIndex key in oldItems)
                {
                    Remove(key);
                }
            }
        }

        private void TouchPermaCache(Dictionary<TileIndex, DateTime> touched)
        {
            List<TileIndex> keys = new List<TileIndex>();
            //This is a temporary solution to preserve level zero tiles in memory.
            foreach (TileIndex key in touched.Keys) if (key.Level == 0) keys.Add(key);
            foreach (TileIndex key in keys) touched[key] = DateTime.Now;
        }

        private static DateTime GetCutOff(Dictionary<TileIndex, DateTime> touched,
          int lowerLimit)
        {
            List<DateTime> times = new List<DateTime>();
            foreach (DateTime time in touched.Values)
            {
                times.Add(time);
            }
            times.Sort();
            return times[times.Count - lowerLimit];
        }

        private static List<TileIndex> GetOldItems(Dictionary<TileIndex, DateTime> touched,
          ref DateTime cutoff)
        {
            List<TileIndex> oldItems = new List<TileIndex>();
            foreach (TileIndex key in touched.Keys)
            {
                if (touched[key] < cutoff)
                {
                    oldItems.Add(key);
                }
            }
            return oldItems;
        }

        #endregion

        #region INotifyPropertyChanged Members

        protected virtual void OnNotifyPropertyChange(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
