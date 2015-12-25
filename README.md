# BotanyEV3Controller
The BotanyEV3Controller is a joint project with the Wisconsin Institute for
Research for a botany outreach program to control a mobile camera platform
recording the growth progress of newly sprouted seeds. The camera platform
uses a Lego EV3 for mobility. This project comes in two components, this
platform controller and a BotanyWebcamController responsible for actually
controlling the webcams. The two programs communicate with each other over
network loopback.

The program's movement pattern is controlled by alternating color patterns
of red and blue spots marking positions to take a photo. After passing the
designated number of positions the platform will go into reverse until it sees
a green spot marking its stop point.
