CREATE OR REPLACE FUNCTION CheckDrawAndResults(
    LotteryId INT,
    DrawDate TIMESTAMP,
    WinningNumbers INT[],
    BonusNumbers INT[]
) RETURNS BOOLEAN AS $$
DECLARE
    DrawExists BOOLEAN;
    DrawResultExists BOOLEAN;
BEGIN
    -- Check if the draw exists
    SELECT EXISTS (
        SELECT 1 FROM Draws
        WHERE LotteryId = LotteryId AND DrawDate = DrawDate
    ) INTO DrawExists;

    IF NOT DrawExists THEN
        RETURN FALSE;
    END IF;

    -- Check if the draw results exist
    SELECT EXISTS (
        SELECT 1 FROM DrawResults dr
        JOIN Draws d ON dr.DrawId = d.DrawId
        WHERE d.LotteryId = LotteryId AND d.DrawDate = DrawDate
        AND dr.WinningNumbers = WinningNumbers
        AND (dr.BonusNumbers IS NULL OR dr.BonusNumbers = BonusNumbers)
    ) INTO DrawResultExists;

    RETURN DrawResultExists;
END;
$$ LANGUAGE plpgsql;
