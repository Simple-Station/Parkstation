## wizard
# Shown at the end of a round of wizard
wizard-round-end-result = {$wizardCount ->
    [one] There was one wizard.
    *[other] There were {$wizardCount} wizards.
}

# Shown at the end of a round of wizard
wizard-user-was-a-wizard = [color=gray]{$user}[/color] was a wizard.
wizard-user-was-a-wizard-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a wizard.
wizard-was-a-wizard-named = [color=White]{$name}[/color] was a wizard.

wizard-user-was-a-wizard-with-objectives = [color=gray]{$user}[/color] was a wizard who had the following objectives:
wizard-user-was-a-wizard-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) was a wizard who had the following objectives:
wizard-was-a-wizard-with-objectives-named = [color=White]{$name}[/color] was a wizard who had the following objectives:

preset-wizard-objective-issuer-wizfeds = [color=#87cefa]The Wizard Federation[/color]

wizard-objective-condition-success = {$condition} | [color={$markupColor}]Success![/color]
wizard-objective-condition-fail = {$condition} | [color={$markupColor}]Failure![/color] ({$progress}%)

preset-wizard-title = wizard
preset-wizard-description = Wizards are hiding among the station crew, find and deal with them before they become too powerful.
preset-wizard-not-enough-ready-players = Not enough players readied up for the game! There were {$readyPlayersCount} players readied up out of {$minimumPlayers} needed.
preset-wizard-no-one-ready = No players readied up! Can't start wizard.

## wizardRole

# wizardRole
wizard-role-greeting =
    You are an undercover operative from the Wizard Federation.
    Your objectives and codewords are listed in the character menu.
    Use the uplink loaded into your PDA to buy the tools you'll need for this mission.
    You can get more currency by completing The Oracle's quests.
    Death to Nanotrasen!
wizard-role-codewords =
    The codewords are:
    {$codewords}.
    Codewords can be used in regular conversation to identify yourself discretely to other operatives.
    Listen for them, and keep them secret.
