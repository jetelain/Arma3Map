using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapExportExtension
{
	public class MapInfos
	{
		public string fullMapTile { get; set; }
		public string worldName { get; set; }
		public string tilePattern { get; set; }
		public int maxZoom { get; set; }
		public int minZoom { get; set; }
		public int defaultZoom { get; set; }
		public string attribution { get; set; }
		public int tileSize { get; set; }
		public List<int> center { get; set; }
		public double worldSize { get; set; }
		public string preview { get; set; }
		public string dlc { get; set; }
		public string title { get; set; }
		public string steamWorkshop { get; set; }
		public List<CityInfos> cities { get; set; }
	}
}
