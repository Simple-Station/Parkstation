buildAllDebug
    Builds all projects with debug configuration
buildAllRelease
    Builds all projects with release configuration

configDev
    Sets the game server config for development
configMap
    Sets the game server config for mapping
configServer
    Sets the game server config to match the actual server

runBuildAll
    Builds the client and server then runs them

runQuickAll
    Runs the client and server without building
runQuickClient
    Runs the client without building
runQuickServer
    Runs the server without building


A quick explanation on the variations of files.

The debug vs release build is simply what people dev in vs the actual server. The release build contains various 
optimizations, while the debug build contains debugging tools. 
If you're mapping, use the release build as it will run smoother.

The Server config file simply matches the actual server, while the other two have some cvar tweaks that come in 
handy for their specific tasks. 
Essentially only saves you some commands when you load in, but very convenient none the less.