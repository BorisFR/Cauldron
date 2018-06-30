﻿// 
// Author: Boris
// Create: 17/06/2018
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Cauldron
{


    public class Tiled
    {

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        public int StartHouse { get; private set; }
        public int StartDoorGreen { get; private set; }
        public int StartDoorBlue { get; private set; }
        public int StartDoorRed { get; private set; }
        public int StartDoorPurple { get; private set; }
        public int StartY { get; private set; }
        public int MapMinX { get; private set; }
        public int MapMaxX { get; private set; }

        public Int16[,] Terrain;
        public Int16[,] Items;
        public Dictionary<int, Tile> Tiles = new Dictionary<int, Tile>();

        public Tiled()
        {
        }

        public void Load(Stream stream)
        {
            // inspired by
            // https://github.com/marshallward/TiledSharp/blob/master/TiledSharp/src/Map.cs
            XDocument xDoc = XDocument.Load(stream);
            XElement xMap = xDoc.Element("map");
            MapWidth = (int)xMap.Attribute("width");
            MapHeight = (int)xMap.Attribute("height");
            System.Diagnostics.Debug.WriteLine(String.Format("Map: {0}x{1}", MapWidth, MapHeight));
            Terrain = new Int16[MapWidth, MapHeight];
            TileWidth = (int)xMap.Attribute("tilewidth");
            TileHeight = (int)xMap.Attribute("tileheight");
            System.Diagnostics.Debug.WriteLine(String.Format("Tile: {0}x{1}", TileWidth, TileHeight));
            Items = new Int16[MapWidth, MapHeight];

            foreach (XElement xLayer in xMap.Elements("layer"))
            {
                string name = (string)xLayer.Attribute("name");
                switch (name)
                {
                    case "terrain":
                        ProcessTerrain(xLayer.Element("data"));
                        break;
                    case "items":
                        ProcessItems(xLayer.Element("data"));
                        break;
                }
            }
        } // Load

        private void ProcessTerrain(XElement element)
        {
            int x, y, width, height, index;
            string[] values;
            bool chunkPresent = false;
            MapMinX = MapWidth;
            MapMaxX = 0;
            foreach (XElement xChunk in element.Elements("chunk"))
            {
                chunkPresent = true;
                x = (int)xChunk.Attribute("x");
                y = (int)xChunk.Attribute("y");
                //System.Diagnostics.Debug.Write(String.Format(" {0}x{1}", x, y));
                width = (int)xChunk.Attribute("width");
                height = (int)xChunk.Attribute("height");
                values = xChunk.Value.Replace("\n", "").Split(',');
                index = 0;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        // position de la maison
                        if (values[index].Equals("701"))
                        {
                            StartHouse = x + i - 6;
                            StartY = y + j + 3;
                            System.Diagnostics.Debug.WriteLine(String.Format("House @ {0}x{1}", StartHouse, StartY));
                        }
                        else if (values[index].Equals("11")) // limite gauche/droite du terrain
                        {
                            if ((x + i) > MapMaxX)
                            {
                                MapMaxX = x + i;
                            }
                            else
                            {
                                if ((x + i) < MapMinX)
                                    MapMinX = x + i;
                            }
                        }

                        Terrain[x + i, y + j] = Convert.ToInt16(values[index]);
                        index++;
                        if (Tiles.ContainsKey(Terrain[x + i, y + j]))
                        {
                            switch (Tiles[Terrain[x + i, y + j]].Name)
                            {
                                case "position":
                                    switch (Tiles[Terrain[x + i, y + j]].Content)
                                    {
                                        case "door_red":
                                            StartDoorRed = x + i - 11;
                                            break;
                                        case "door_blue":
                                            StartDoorBlue = x + i - 11;
                                            break;
                                        case "door_green":
                                            StartDoorGreen = x + i - 11;
                                            break;
                                        case "door_purple":
                                            StartDoorPurple = x + i - 11;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            if (!chunkPresent)
            {
                MapMinX = MapWidth;
                MapMaxX = 0;
                values = element.Value.Replace("\n", "").Split(',');
                index = 0;
                for (int j = 0; j < MapHeight; j++)
                {
                    for (int i = 0; i < MapWidth; i++)
                    {
                        // position du pot dans la maison
                        if (values[index].Equals("1165"))
                        {
                            StartHouse = i - 2;
                            StartY = j - 2;
                            System.Diagnostics.Debug.WriteLine(String.Format("Vial @ {0}x{1}", StartHouse, StartY));
                        }
                        else if (values[index].Equals("11")) // limite gauche/droite du terrain
                        {
                            if (i > MapMaxX)
                            {
                                MapMaxX = i;
                            }
                            else
                            {
                                if (i < MapMinX)
                                    MapMinX = i;
                            }
                        }
                        Terrain[i, j] = Convert.ToInt16(values[index]);
                        index++;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(String.Format("Map from {0} to {1}", MapMinX, MapMaxX));
        } // ProcessTerrain

        private void ProcessItems(XElement element)
        {
            int x, y, width, height, index;
            string[] values;
            foreach (XElement xChunk in element.Elements("chunk"))
            {
                x = (int)xChunk.Attribute("x");
                y = (int)xChunk.Attribute("y");
                width = (int)xChunk.Attribute("width");
                height = (int)xChunk.Attribute("height");
                values = xChunk.Value.Replace("\n", "").Split(',');
                index = 0;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        if (!values[index].Equals("0"))
                        {
                            Items[x + i, y + j] = Convert.ToInt16(values[index]);
                        }
                        index++;
                    }
                }
            }
        } // ProcessItems

        public void ProcessTileSet(Stream stream)
        {
            int i;
            string name, content;
            XDocument xDoc = XDocument.Load(stream);
            XElement xTileSet = xDoc.Element("tileset");
            foreach (XElement xTile in xTileSet.Elements("tile"))
            {
                i = (int)xTile.Attribute("id");
                foreach (XElement xProperties in xTile.Elements("properties"))
                {
                    foreach (XElement xProperty in xProperties.Elements("property"))
                    {
                        name = (string)xProperty.Attribute("name");
                        content = (string)xProperty.Attribute("value");
                        Tiles.Add(i, new Tile() { Name = name, Content = content });
                        System.Diagnostics.Debug.WriteLine(String.Format("Tile: {0} => {1}={2}", i, name, content));
                    }
                }
            }
        } // ProcessTileSet

    }
}