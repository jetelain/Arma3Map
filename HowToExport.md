# How to export a map for Arma3Map

-----

This procedure is deprecated, see https://github.com/jetelain/GameMapStorage.Arma3/wiki for the up to date procedure.

-----

Note: You need a computer with at least 8 GB of memory (exporter will need up to 2GB to be added to game requirements).

1. Download Export mod on Workshop : https://steamcommunity.com/sharedfiles/filedetails/?id=2403150951

2. Disable Battleye, Launch Arma3 with CBA_A3, ACE, the Export mod and all additonal mods required for map to export. 

3. Ensures that Arma 3 has 1920x1080 resolution, Format Auto, and Normal size UI.

4. Open Eden Editor, place any unit on map, ensures that difficulity is set to veteran (to avoid any marker on map), and launch mission

5. Open map, disable satellite view / textures, place cursor out of map surface, and hit key `Home` (Might be ↖, ◤, ⇱, Pos1, or something like that), "Taking screenshots..." should be displayed, and map should be moving

6. Wait for up to 10 minutes (Altis took 7 minutes on my computer) until "It's done" is displayed

7. Close game, open `%USERPROFILE%\Arma3MapExporter` with explorer, all data is ready.

8. To test, copy folder `js` and `css` from git repo to `%USERPROFILE%\Arma3MapExporter`, and then open the `.html` file.

9. Create a merge request on git hub !
