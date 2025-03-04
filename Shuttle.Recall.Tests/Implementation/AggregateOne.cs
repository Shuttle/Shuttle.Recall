﻿namespace Shuttle.Recall.Tests.Implementation;

public class AggregateOne
{
    public string ThatValue { get; private set; } = string.Empty;
    public string ThisValue { get; private set; } = string.Empty;

    public ThatHappened DoThat(string value)
    {
        return On(new ThatHappened
        {
            ThatValue = value
        });
    }

    public ThisHappened DoThis(string value)
    {
        return On(new ThisHappened
        {
            ThisValue = value
        });
    }

    private ThisHappened On(ThisHappened thisHappened)
    {
        ThisValue = thisHappened.ThisValue;

        return thisHappened;
    }

    private ThatHappened On(ThatHappened thisHappened)
    {
        ThatValue = thisHappened.ThatValue;

        return thisHappened;
    }
}