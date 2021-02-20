#include "script_component.hpp"

INFO("PostInit");

addMissionEventHandler ["ExtensionCallback", {
	params ["_name", "_function", "_data"];
	if ( _name == "a3me" ) then {
		if( _function == "Log" ) exitWith {
			LOG(_data);
		};
		if( _function == "Error" ) exitWith {
			ERROR(_data);
		};
	};
}];


"mapExportExtension" callExtension "Warmup";

a3me_calibrate = 
{
	INFO("Calibrate");

	private _deltaX = 500;
	private _deltaY = 500;
	private _w = 1000;
	private _h = 1000;
	private _control = (findDisplay 12) displayCtrl 51;

	private _zoom = getNumber (configFile >> "CfgWorlds" >> worldName >> "Grid" >> "Zoom1" >> "zoomMax") * 1.01; // FIXME: does not work on all maps...

	_control ctrlMapAnimAdd [0, _zoom, [_deltaX,_deltaY]];
	ctrlMapAnimCommit _control;
	sleep 0.1;

	private _posA = _control ctrlMapWorldToScreen [0,0];
	private _posB = _control ctrlMapWorldToScreen [_w,_h];

	private _args = [
		[safeZoneX, safeZoneY, safeZoneW, safeZoneH],
		_posA,
		_posB,
		_w,
		_h
	];

	INFO_1("calibrate(%1)",_args);
	"mapExportExtension" callExtension ["calibrate", _args];

	[_zoom, _deltaX, _deltaY, _h, _w]
};


a3me_screenShotLoop = 
{
	INFO("ScreenShotLoop");

	params ["_zoom","_deltaX", "_deltaY", "_w", "_h"];
	private _x = 0;
	private _y = 0;
	private _control = (findDisplay 12) displayCtrl 51;
	while { _x <= worldSize } do {
		_y = 0;
		while { _y <= worldSize } do {
		
			_control ctrlMapAnimAdd [0, _zoom, [_x+_deltaX,_y+_deltaY]];
			ctrlMapAnimCommit _control;
			sleep 0.25;

			private _posA = _control ctrlMapWorldToScreen [_x,_y];
			private _posB = _control ctrlMapWorldToScreen [_x+_w,_y+_h];
			private _args = [_x,_y,_posA,_posB];
			INFO_1("screenshot(%1)",_args);
			"mapExportExtension" callExtension ["screenshot", _args];
			_y = _y + _h;
		};
		_x = _x + _w;
	};
};

a3me_export = {
	INFO("Export");

	private _center = getArray (configFile >> "CfgWorlds" >> worldName >> "centerPosition");
	private _title = getText (configFile >> "CfgWorlds" >> worldName >> "description");
	
	private _cities = []; 
	{
		_cities pushBack [text _x, locationPosition _x];
	} forEach nearestLocations [_center, ["NameCity", "NameCityCapital", "NameVillage"], 25000];

	INFO("Start");

	"mapExportExtension" callExtension ["start", [worldName, worldSize, _cities, _center, _title]];

	(call a3me_calibrate) call a3me_screenShotLoop;

	INFO("Stop");
	"mapExportExtension" callExtension ["stop", [worldName, worldSize]];

	systemChat "It's done !";
};

#include "\a3\ui_f\hpp\defineDIKCodes.inc"

["Arma3 Map Export", "Launch export", ["Launch export", "Launch export"], {}, { [] spawn a3me_export; }, [DIK_HOME, [false, false, false]]] call CBA_fnc_addKeybind;
