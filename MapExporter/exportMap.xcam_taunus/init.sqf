
sleep 10; // laisse le temps d'afficher la carte en jeu, et de désactiver la photo aérienne

private _zoom=0.11125;
private _control = (findDisplay 12) displayCtrl 51;
private _x = 1625;
private _y = 1125;
private _posX = 0;
private _posY = 0;


while { _x < 23500 } do {
	_y = 1125;
	while { _y < 22500 } do {
	
		_control ctrlMapAnimAdd [0, _zoom, [_x,_y]];
		ctrlMapAnimCommit _control;
		sleep 2;
		
		_y = _y + 2000;
		_posY = _posY + 3;
	};
	_x = _x + 3000;
	_posX = _posX + 3;
};

