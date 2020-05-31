
sleep 5;

private _zoom=0.148;
private _control = (findDisplay 12) displayCtrl 51;
private _x = 1625;
private _y = 1025;
private _posX = 0;
private _posY = 0;


while { _x < 16000 } do {
	_y = 1025;
	while { _y < 16000 } do {
	
		_control ctrlMapAnimAdd [0, _zoom, [_x,_y]];
		ctrlMapAnimCommit _control;
		sleep 2;


		_y = _y + 2000;
		_posY = _posY + 3;
	};
	_x = _x + 3000;
	_posX = _posX + 3;
};
