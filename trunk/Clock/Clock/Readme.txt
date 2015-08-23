Zune Clock 1.5

Description: Clock which displays the date and time with media player abilities. It allows adjusting the date and time displayed.

Features:
- Clock
	- Displays the time
	- Displays the date
	- Allows setting the date/time
- Media player
	- Up/Down adjusts the volume
	- Left/Right moves to the previous/next song
	- Displays the album art
	- Displays the artist and the song title
	- Can select a playlist directly from the application (settings)
	- Can set the shuffle mode (settings)
- Custom background
	- Can specify any picture from the Zune as custom background (you need to have a few pictures first)

Controls:
- Back Button - exits the application
- Up/Down - Volume
- Left/Right - Previous/Next song
- Pause Button - Pause/resume
- Click on the zune pad - enters setting mode
	* Back - exits setting mode
	* Up/Down - increments the selected setting
	* Left/Right - cycles through the different settings (background, hours, minutes, seconds, days) 
	* Click on the Zune pad - resets the current setting to the default value
		- When a playlist is selected - plays the selected playlist
		- When setting the time: sets the offset to 0 (so that you will see the hardware date/time)
		- When setting the background: sets the background to "No background"
	* Pause/Back - exits setting mode
	
More details:
http://blogs.msdn.com/adrianvinca

Adrian Vinca 2008

Changelist
1.5
	New features and improvements:
	- New feature: Media player
		- Up/Down adjusts the volume
		- Left/Right moves to the previous/next song
		- Displays the album art
		- Displays the artist and the song title
	- New feature: Can select a playlist to play it
	- New feature: Dim the background while in setting mode
	- Update: Changed some controls for the setting mode (to make space for the pause button - which was needed for the media player):
		- Click on the zune pad to enter the settings mode (previously it was the pause button)
		- Back button to exit settings mode (previousely also the pause button used to do that)
	- Bug: Fixed some possible crashes if the name of the background has non-latin characaters
	

1.1
	New features and improvements:
	- New feature: Background customization (you need to have a few pictures on the Zune)
	- Bug: Fixed a bug which caused the settings data to become corrupt in some cases
	- Bug: Performance improvements (application loops slower than initially - a clock doesn't need a very high refresh rate)
	- Update: Using white color for the settings (it makes the text more visible when having a custom background)
	- Update: The application version is displayed when in setting mode
	- Code fix: Fixed the sources so that they compile fine on Debug
	- Code fix: Refactored some of the settings code to make space for future settings
	
1.0
- the initial implementation