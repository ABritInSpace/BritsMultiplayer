#ABrit's Multiplayer Mod#

##What is this?##
ABrit's Multiplayer Mod aims to test multiplayer functionality in KSP 1, to prepare for KSP 2 modding around launch day, and beat Intercept Games to the punch! The idea is to introduce dedicated server functionality, while adopting certain features / design from other KSP multiplayer mods (e.g LunaMultiplayer and DarkMultiplayer) without the additional complexity.

##How does it work?##
The idea is to use an external client process to sync with the game, provided with updates from the game on the current active vessel in control (and not spectated) over TCP and UDP - TCP for initial connection and syncing vessels on rails, and UDP for syncing close-by craft. This way, dedicated connections can be made between clients through a server without much latency, and therefore a smoother experience can be had.

To tackle the syncing issues with timewarp, timewarp "subspaces" can be made and desync with these subspaces can be checked and corrected (with one client at priority, most likely the lowest latency or load). Players can sync with these sub-spaces and see the active vessels within. All vessels are eventually saved to the latest subspace whenever a player leaves from an older subspace (you can't travel back in time!).

##Will all features be added?##
Probably not! This is moreso a test project for my own personal ability, and not intended as a complete solution.

To see the feature list and an in-depth decomposition of the project, go to [overview](https://github.com/ABritInSpace/BritsMultiplayer/blob/main/overview).