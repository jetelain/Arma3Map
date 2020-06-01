
function InitMap(mapInfos) {
    $(function () {

        var map = L.map('map', {
            minZoom: mapInfos.minZoom,
            maxZoom: mapInfos.maxZoom,
            crs: mapInfos.CRS
        });

        L.tileLayer('.'+mapInfos.tilePattern, {
            attribution: mapInfos.attribution,
            tileSize: mapInfos.tileSize
        }).addTo(map);

        map.setView(mapInfos.center, mapInfos.defaultZoom);

        L.latlngGraticule().addTo(map);

        L.control.scale({ maxWidth: 200, imperial: false }).addTo(map);

		L.control.gridMousePosition().addTo(map);
		
		if (window.location.hash == '#cities' ) 
		{
			$.each(mapInfos.cities, function(index, city){
				
				L.marker([city.y, city.x]).addTo(map).bindPopup(city.name);
			});
		}
    });
}