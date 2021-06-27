#include "script_component.hpp"

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