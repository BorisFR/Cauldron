// 
// Author: Boris
// Create: 17/06/2018
// 

using System;
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

        public int[,] Terrain;

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
            Terrain = new int[MapWidth, MapHeight];
            TileWidth = (int)xMap.Attribute("tilewidth");
            TileHeight = (int)xMap.Attribute("tileheight");
            System.Diagnostics.Debug.WriteLine(String.Format("Tile: {0}x{1}", TileWidth, TileHeight));

            foreach (XElement xLayer in xMap.Elements("layer"))
            {
                string name = (string)xLayer.Attribute("name");
                switch (name)
                {
                    case "terrain":
                        processTerrain(xLayer.Element("data"));
                        break;
                    case "items":
                        processItems(xLayer.Element("data"));
                        break;
                }
            }
        }

        public void processTerrain(XElement element)
        {
            int x, y, width, height, index;
            string[] values;
            foreach (XElement xChunk in element.Elements("chunk"))
            {
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
                            StartHouse = x + i;
                        }

                        Terrain[x + i, y + j] = Convert.ToInt32(values[index]);
                        index++;
                    }
                }
            }
        }

        public void processItems(XElement element)
        {
            int x, y, width, height, index;
            string[] values;
            foreach (XElement xChunk in element.Elements("chunk"))
            {
                x = (int)xChunk.Attribute("x");
                y = (int)xChunk.Attribute("y");
                width = (int)xChunk.Attribute("width");
                height = (int)xChunk.Attribute("height");
                values = xChunk.Value.Split(',');
                index = 0;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        // x+i  /  y+j
                        //values[index];

                        index++;
                    }
                }
            }
        }

    }
}