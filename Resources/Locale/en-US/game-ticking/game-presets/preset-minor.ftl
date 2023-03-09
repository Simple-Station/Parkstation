## flawed

# Shown at the end of a round of flawed antags
flawed-round-end-result = {$flawedCount ->
    [one] There was one flawed antag.
    *[other] There were {$flawedCount} flawed antags.
}

# Shown at the end of a round of flawed antags
flawed-user-was-a-flawed = [color=gray]{$user}[/color] was a flawed antag.
flawed-user-was-a-flawed-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a flawed antag.
flawed-was-a-flawed-named = [color=White]{$name}[/color] was a flawed antag.

flawed-user-was-a-flawed-with-objectives = [color=gray]{$user}[/color] was a flawed antag who had the following objective:
flawed-user-was-a-flawed-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a flawed antag who had the following objectives:
flawed-was-a-flawed-with-objectives-named = [color=White]{$name}[/color] was a flawed antag who had the following objectives:

preset-flawed-objective-issuer-flawed = [color=#87cefa]Their own flaws[/color]

flawed-objective-condition-success = {$condition} | [color={$markupColor}]Success![/color]
flawed-objective-condition-fail = {$condition} | [color={$markupColor}]Failure![/color] ({$progress}%)

flawed-title = flawed antag
flawed-description = This gamemode shouldn't be used..
flawed-not-enough-ready-players = Not enough players readied up for the game! There were {$readyPlayersCount} players readied up out of {$minimumPlayers} needed.
flawed-no-one-ready = No players readied up! Can't start flawed antag.

## flawedRole

# flawedRole
flawed-role-greeting =
    You are an agent of your own flaws.
