// 
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
        public List<Respawn> Respawns = new List<Respawn>();
        public int StartY { get; private set; }
        public int MapMinX { get; private set; }
        public int MapMaxX { get; private set; }
        public int MapMinY { get; private set; }
        public int MapMaxY { get; private set; }

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
                        /*if (values[index].Equals("701"))
                        {
                            StartHouse = x + i - 6;
                            StartY = y + j + 3;
                            System.Diagnostics.Debug.WriteLine(String.Format("House @ {0}x{1}", StartHouse, StartY));
                        }*/
                        Terrain[x + i, y + j] = Convert.ToInt16(values[index]);
                        index++;
                        DoCheckTerrain(x + i, y + j);
                    }
                }
            }
            if (!chunkPresent)
            {
                values = element.Value.Replace("\n", "").Split(',');
                index = 0;
                for (int j = 0; j < MapHeight; j++)
                {
                    for (int i = 0; i < MapWidth; i++)
                    {
                        // position du pot dans la maison
                        if (values[index].Equals("1165"))
                        {
                            //StartHouse = i - 2;
                            //StartY = j - 2;
                            System.Diagnostics.Debug.WriteLine(String.Format("Vial @ {0}x{1}", i - 2, j - 2));
                        }
                        Terrain[i, j] = Convert.ToInt16(values[index]);
                        index++;
                        DoCheckTerrain(i, j);
                    }
                }
            }
        } // ProcessTerrain

        private void DoCheckTerrain(int x, int y)
        {
            if (All.tiled.Tiles.ContainsKey(Terrain[x, y]))
            {
                switch (All.tiled.Tiles[Terrain[x, y]].Name)
                {
                    case "generator": // use when drawing screen
                        break;
                    case "position":
                        switch (All.tiled.Tiles[Terrain[x, y]].Content)
                        {
                            case "door_red":
                                StartDoorRed = x - 11;
                                System.Diagnostics.Debug.WriteLine(String.Format("Red door @ {0}", x));
                                break;
                            case "door_blue":
                                StartDoorBlue = x - 11;
                                System.Diagnostics.Debug.WriteLine(String.Format("Blue door @ {0}", x));
                                break;
                            case "door_green":
                                StartDoorGreen = x - 12;
                                System.Diagnostics.Debug.WriteLine(String.Format("Green door @ {0}", x));
                                break;
                            case "door_purple":
                                StartDoorPurple = x - 12;
                                System.Diagnostics.Debug.WriteLine(String.Format("Purple door @ {0}", x));
                                break;
                            case "house":
                                StartHouse = x - 12;
                                StartY = y - 2;
                                System.Diagnostics.Debug.WriteLine(String.Format("House @ {0}x{1}", StartHouse, StartY));
                                break;
                        }
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(String.Format("ERROR terrain @ {0}x{1}, name: {2}/{3}", x, y, All.tiled.Tiles[Terrain[x, y]].Name, All.tiled.Tiles[Terrain[x, y]].Content));
                        break;
                }
            }
        }

        private void ProcessItems(XElement element)
        {
            MapMinX = MapWidth;
            MapMaxX = 0;
            MapMinY = MapHeight;
            MapMaxY = 0;
            int x, y, width, height, index;
            string[] values;
            bool chunkPresent = false;
            foreach (XElement xChunk in element.Elements("chunk"))
            {
                chunkPresent = true;
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
                            DoCheckItems(x + i, y + j);
                        }
                        index++;
                    }
                }
            }
            if (!chunkPresent)
            {
                values = element.Value.Replace("\n", "").Split(',');
                index = 0;
                for (int j = 0; j < MapHeight; j++)
                {
                    for (int i = 0; i < MapWidth; i++)
                    {
                        if (!values[index].Equals("0"))
                        {
                            Items[i, j] = Convert.ToInt16(values[index]);
                            DoCheckItems(i, j);
                        }
                        index++;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(String.Format("Map from {0}/{1} to {2}/{3}", MapMinX, MapMinY, MapMaxX, MapMaxY));
        } // ProcessItems

        private void DoCheckItems(int x, int y)
        {
            if (All.tiled.Tiles.ContainsKey(Items[x, y]))
            {
                switch (All.tiled.Tiles[Items[x, y]].Name)
                {
                    case "zone":
                        switch (All.tiled.Tiles[Items[x, y]].Content)
                        {
                            case "border":
                                if (x > MapMaxX)
                                {
                                    MapMaxX = x;
                                }
                                else
                                {
                                    if (x < MapMinX)
                                        MapMinX = x;
                                }
                                if (y > MapMaxY)
                                    MapMaxY = y;
                                else if (y < MapMinY)
                                    MapMinY = y;
                                break;
                            case "blocking":
                            case "landing":
                                break;
                            case "spawn":
                                Respawns.Add(new Respawn() { X = x, Y = y - 2 });
                                System.Diagnostics.Debug.WriteLine(String.Format("Spawn @ {0}x{1}", x, y));
                                break;
                            default:
                                System.Diagnostics.Debug.WriteLine(String.Format("ERROR @ {0}x{1}, Zone Content: {2}", x, y, All.tiled.Tiles[Items[x, y]].Content));
                                break;
                        }
                        break;
                    case "item":
                        break;
                    case "static":
                        break;
                    case "magic":
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(String.Format("ERROR items @ {0}x{1}, name: {2}/{3}", x, y, All.tiled.Tiles[Items[x, y]].Name, All.tiled.Tiles[Items[x, y]].Content));
                        break;
                }
            }
        }

        public void ProcessTileSet(Stream stream)
        {
            int i;
            string name, content;
            XDocument xDoc = XDocument.Load(stream);
            XElement xTileSet = xDoc.Element("tileset");
            foreach (XElement xTile in xTileSet.Elements("tile"))
            {
                i = 1 + (int)xTile.Attribute("id");
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