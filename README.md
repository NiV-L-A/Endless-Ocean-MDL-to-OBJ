## INFO ##

Endless Ocean 1 & 2 .mdl to .obj Exporter written in C# by NiV, with the help of MDB & Hiroshi. Each mesh equals to a .obj. There're no known bugs. Remember that to get .mdl files from Luigi's .bms script, you have to edit the script. Change the line "math AUTO_PARSING_RF2 = 1" to "math AUTO_PARSING_RF2 = 0". This is version 0.6, remember that if you have any issues, join this discord server and contact me (NiV-L-A): https://discord.gg/4hmcsmPMDG - Endless Ocean Wiki Server

## How to run ##
Download the executable, drag and drop the .mdl onto the .exe. It supports multiple .mdl as the input argument, don't abuse it tho, it will make it mad.

## TO DO: ##
- Add an actual command-line interaction. User could choose optional output folder.
	- Could be a cool idea to print on the console at which mesh the script is currently at, instead of the data being logged.
- In stages files, some chunks are rendered but loaded at 0,0,0. Find a way to locate them at their correct position. INFO contains x/z/y float, but the position of the mesh in the world is already given by the vertices.
- Optimize some stuff.
