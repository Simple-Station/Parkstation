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

        if (userMopStats.Count == 0)
        {
            line += "\n[color=red]" + Loc.GetString("eofstats-mop-noamountmopped") + "[/color]";
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
                case var x when x > 1600:
                    impressColor = "blue";
                    break;
                case var x when x > 800:
                    impressColor = "gold";
                    break;
                case var x when x > 300:
                    impressColor = "silver";
                    break;
                default:
                    impressColor = "brown";
                    break;
            }

            line += "\n" + Loc.GetString("eofstats-mop-amountmopped", ("amountMopped", (int) totalAmountMopped), ("timesMopped", timesMopped), ("impressColor", impressColor));

            line += "\n" + Loc.GetString("eofstats-mop-topmopper-header");

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
                    impressColor = "gold";
                    break;
                case 2:
                    impressColor = "silver";
                    break;
                default:
                    impressColor = "burlywood";
                    break;
            }

            if (data.Username != null)
                line += "\n" + Loc.GetString("eofstats-mop-topmopper-hasusername", ("name", data.Name), ("username", data.Username), ("amountMopped", (int) amountMopped), ("impressColor", impressColor), ("place", currentPlace));
            else
                line += "\n" + Loc.GetString("eofstats-mop-topmopper-hasnousername", ("name", data.Name), ("amountMopped", (int) amountMopped), ("impressColor", impressColor), ("place", currentPlace));

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
