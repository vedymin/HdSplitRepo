namespace HdSplit.Models {
    public enum Lines {
        LINE_00,
        LINE_10,
        LINE_11,
        LINE_12,
        LINE_15,
        LINE_16,
        LINE_17,
        LINE_18,
        LINE_19,
        LINE_20,
        LINE_30,
        LINE_40,
        LINE_41,
        LINE_50,
        LINE_52,
        LINE_56,
        LINE_60,
        LINE_70,
        LINE_71,
        LINE_80,
        LINE_81,
        LINE_90,
        LINE_92,
        LINE_93,
        LINE_98
    }

    public enum States
    {
        firstScanOfHd,
        itemScan,
        newHdScan,
        checkHdAfterItemScan
    }

    public enum HdResult
    {
        differentLine,
        hdUnknown,
        hdCorrect,
		unconfirmedPicks
    }
}
