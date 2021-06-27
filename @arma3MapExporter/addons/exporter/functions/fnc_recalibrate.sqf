#include "script_component.hpp"

INFO("ReCalibrate");

params ["_zoom", "_deltaX", "_deltaY", "_h", "_w"];

_zoom = _zoom / 2;
_deltaX = _deltaX / 2;
_deltaY = _deltaY / 2;
_h = _h / 2;
_w = _w / 2;
private _dbg = [_zoom, _deltaX, _deltaY, _h, _w];
INFO_1("%1",_dbg);

private _control = (findDisplay 12) displayCtrl 51;
_control ctrlMapAnimAdd [0, _zoom, [_deltaX,_deltaY]];
ctrlMapAnimCommit _control;
sleep 0.5;

private _posA = _control ctrlMapWorldToScreen [0,0];
private _posB = _control ctrlMapWorldToScreen [_w,_h];

INFO_2("zoom=%1 dx=%2", _zoom, (_posB select 0) - (_posA select 0));

_control ctrlMapAnimAdd [0, _zoom, [_deltaX,_deltaY]];
ctrlMapAnimCommit _control;
sleep 0.5;

_posA = _control ctrlMapWorldToScreen [0,0];
_posB = _control ctrlMapWorldToScreen [_w,_h];

INFO_2("zoom=%1 dx=%2", _zoom, (_posB select 0) - (_posA select 0));

private _args = [
	[safeZoneX, safeZoneY, safeZoneW, safeZoneH],
	_posA,
	_posB,
	_w,
	_h
];

INFO_2("re-calibrate(%1) %2",_args,_dbg);
"mapExportExtension" callExtension ["calibrate", _args];

[_zoom, _deltaX, _deltaY, _h, _w]