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


a3me_export = {

	systemChat "Taking screenshots...";

	INFO("Export");

	private _center = getArray (configFile >> "CfgWorlds" >> worldName >> "centerPosition");
	private _title = getText (configFile >> "CfgWorlds" >> worldName >> "description");
	
	private _cities = []; 
	{
		_cities pushBack [text _x, locationPosition _x];
	} forEach nearestLocations [_center, ["NameCity", "NameCityCapital", "NameVillage"], 25000];

	INFO("Start");

	"mapExportExtension" callExtension ["start", [worldName, worldSize, _cities, _center, _title]];

	private _calibrateData = [1000] call FUNC(calibrate);

	_calibrateData call FUNC(screenShotLoop);

	systemChat "Generate tiles...";
	sleep 0.2;

	INFO("Stop");
	"mapExportExtension" callExtension ["stop", [worldName, worldSize]];

	systemChat "Taking screenshots for HiRes...";

	INFO("Start");

	"mapExportExtension" callExtension ["histart", [worldName, worldSize]];

	(_calibrateData call FUNC(recalibrate)) call FUNC(screenShotLoop);

	systemChat "Generate tiles for HiRes...";
	sleep 0.2;

	INFO("Stop");
	"mapExportExtension" callExtension ["histop", [worldName, worldSize]];

	systemChat "It's done !";

	"mapExportExtension" callExtension ["dispose", [worldName, worldSize]];
};

#include "\a3\ui_f\hpp\defineDIKCodes.inc"

["Arma3 Map Export", "Launch export", ["Launch export", "Launch export"], {}, { [] spawn a3me_export; }, [DIK_HOME, [false, false, false]]] call CBA_fnc_addKeybind;
