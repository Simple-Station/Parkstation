## minor

# Shown at the end of a round of minor
minor-round-end-result = {$minorCount ->
    [one] There was one minor.
    *[other] There were {$minorCount} minors.
}

# Shown at the end of a round of minor
minor-user-was-a-minor = [color=gray]{$user}[/color] was a minor.
minor-user-was-a-minor-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a minor.
minor-was-a-minor-named = [color=White]{$name}[/color] was a minor.

minor-user-was-a-minor-with-objectives = [color=gray]{$user}[/color] was a minor who had the following objective:
minor-user-was-a-minor-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a minor who had the following objectives:
minor-was-a-minor-with-objectives-named = [color=White]{$name}[/color] was a minor who had the following objectives:

preset-minor-objective-issuer-freewill = [color=#87cefa]Free Will[/color]

minor-objective-condition-success = {$condition} | [color={$markupColor}]Success![/color]
minor-objective-condition-fail = {$condition} | [color={$markupColor}]Failure![/color] ({$progress}%)

minor-title = minor
minor-description = This gamemode shouldn't be used..
minor-not-enough-ready-players = Not enough players readied up for the game! There were {$readyPlayersCount} players readied up out of {$minimumPlayers} needed.
minor-no-one-ready = No players readied up! Can't start minor.

## minorRole

# minorRole
minor-role-greeting =
    You are an agent of your free will.
