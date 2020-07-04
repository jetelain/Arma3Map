
sleep 5;

private _zoom=0.17;
private _control = (findDisplay 12) displayCtrl 51;
private _x = 1225;
private _y = 975;

while { _x < 14000 } do {
	_y = 975;
	while { _y < 14000 } do {
	
		_control ctrlMapAnimAdd [0, _zoom, [_x,_y]];
		ctrlMapAnimCommit _control;
		sleep 2;


		_y = _y + 2000;
	};
	_x = _x + 2000;
};


private _result = []; 
{
	_result pushBack [text _x, locationPosition _x];
} forEach nearestLocations [getArray (configFile >> "CfgWorlds" >> worldName >> "centerPosition"), ["NameCity", "NameCityCapital", "NameVillage"], 25000];
copyToClipboard str _result;

// \[("[^"]+")\,\[([0-9\.\-]+),([0-9\.\-]+),[0-9\.\-]+\]\]
// {name:$1,x:$2,y:$3}
