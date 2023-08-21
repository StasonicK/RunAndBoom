Thank you for downloading the Editor Audio Player by Madboy Studios!

The Audio Player provides an option to listen to sound files in edit mode without needing to set up an audio source.
It works by generating, managing, and cleaning up a gameobject with an audio source while in edit mode.

Project on Github: https://github.com/DevJamesC/UnityEditorAudioPlayer
Feature Overview on Youtube: https://www.youtube.com/watch?v=TxsoulK5D2k&t=2s&ab_channel=MadboyStudios



Accessing the Audio Player
1. Open the Editor Audio Player window by finding the "Tools" tab in the top left menu.
2. Tools->Madboy Studios->Editor Audio Player



List of Controls

Play: Plays the clip manually or automatically set in the "File" field

Pause: Pauses the currently playing sound object.

Stop: Destroys the currently playing sound object.

Previous: Selects the previous clip in the playlist. Will auto-play if the current clip was playing.

Next: Selects the next clip in the playlist. Will auto-play if the current clip was playing.

Playback Progress: Shows the current playtime of the clip, a progress slider, and the total playtime of the clip.

Volume: Shows a progress slider, and the current volume level.

Loop Selection: Toggles wether or not to loop the playlist when the last clip plays.

Play Selected Files: Toggles wether or not to automatically populate the playlist by the files selected in the project tab.

Play File When Selected: Toggles wether or not to auto-play a playlist when it is selected in the project tab.

File: Outlines the file currently queued or playing.


Notes
1. When selecting a sound file, unity does some inital loading and caching when queuing up the playlist. This is usually instantanious with single files, but will induce some lag or freezing on large selections when selected for the first time.