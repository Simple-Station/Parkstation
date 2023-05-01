using Content.Server.GameTicking;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;

namespace Content.Server.SimpleStation14.EndOfRoundStats.MopUsed;

public sealed class MopUsedStatSystem : EntitySystem
{
    Dictionary<MopperData, FixedPoint2> userMopStats = new();
    int timesMopped = 0;

    int currentPlace = 1; // For making the top moppers.

    public struct MopperData
    {
        public String Name;
        public String? Username;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MopUsedStatEvent>(OnMopUsed);

        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }


    private void OnMopUsed(MopUsedStatEvent args)
    {
        timesMopped++;

        var mopperData = new MopperData
        {
            Name = args.Mopper,
            Username = args.Username
        };

        if (userMopStats.ContainsKey(mopperData))
        {
            userMopStats[mopperData] += args.AmountMopped;
            return;
        }

        userMopStats.Add(mopperData, args.AmountMopped);
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        var line = String.Empty;

        line += "\n\n" + "[color=green]" + "Mopping statistics!" + "[/color]";

        if (userMopStats.Count == 0)
        {
            line += "\n   [color=red]" + "Not one puddle was mopped this round!" + "[/color]";
        }
        else
        {
            FixedPoint2 totalAmountMopped = 0;

            (MopperData, FixedPoint2) topMopperOne = (new MopperData(), 0);
            (MopperData, FixedPoint2) topMopperTwo = (new MopperData(), 0);
            (MopperData, FixedPoint2) topMopperThree = (new MopperData(), 0);

            foreach (var (mopper, amountMopped) in userMopStats)
            {
                totalAmountMopped += amountMopped;

                if (amountMopped > topMopperOne.Item2)
                {
                    topMopperThree = topMopperTwo;
                    topMopperTwo = topMopperOne;
                    topMopperOne = (mopper, amountMopped);
                }
                else if (amountMopped > topMopperTwo.Item2)
                {
                    topMopperThree = topMopperTwo;
                    topMopperTwo = (mopper, amountMopped);
                }
                else if (amountMopped > topMopperThree.Item2)
                {
                    topMopperThree = (mopper, amountMopped);
                }
            }

            String impressColor;

            switch (timesMopped)
            {
                case var x when x > 1200:
                    impressColor = "[color=blue]";
                    break;
                case var x when x > 600:
                    impressColor = "[color=gold]";
                    break;
                case var x when x > 200:
                    impressColor = "[color=silver]";
                    break;
                default:
                    impressColor = "[color=brown]";
                    break;
            }

            totalAmountMopped = (int) totalAmountMopped;
            line += "\n   " + "A total of " + impressColor + totalAmountMopped + "[/color]" + " units of liquid was mopped this round across " + impressColor + timesMopped + "[/color]" + " puddles!";

            line += "\n\n   " + "The top moppers were:";

            line += GenerateTopMopper(topMopperOne.Item1, topMopperOne.Item2);
            line += GenerateTopMopper(topMopperTwo.Item1, topMopperTwo.Item2);
            line += GenerateTopMopper(topMopperThree.Item1, topMopperThree.Item2);
        }

        ev.AddLine(line);
    }

    private String GenerateTopMopper(MopperData data, FixedPoint2 amountMopped)
    {
        var line = String.Empty;

        if (amountMopped > 0)
        {
            String impressColor;

            switch (currentPlace)
            {
                case 1:
                    impressColor = "[color=gold]";
                    break;
                case 2:
                    impressColor = "[color=silver]";
                    break;
                default:
                    impressColor = "[color=brown]";
                    break;
            }

            if (data.Username == null)
                line += "\n   " + impressColor + currentPlace + ". " + data.Name + " with " + (int) amountMopped + " units mopped!" + "[/color]";
            else
                line += "\n   " + impressColor + currentPlace + ". " + data.Username + " as " + data.Name + " with " + (int) amountMopped + " units mopped!" + "[/color]";

            currentPlace++;
        }

        return line;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        userMopStats.Clear();
        timesMopped = 0;
        currentPlace = 1;
    }
}
