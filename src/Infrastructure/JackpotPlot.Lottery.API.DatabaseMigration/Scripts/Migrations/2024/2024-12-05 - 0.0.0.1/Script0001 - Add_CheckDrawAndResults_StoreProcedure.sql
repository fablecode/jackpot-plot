CREATE OR REPLACE PROCEDURE public.checkdrawandresults(
	IN lotteryid integer,
	IN drawdate timestamp without time zone,
	IN winningnumbers integer[],
	IN bonusnumbers integer[],
	OUT result boolean)
LANGUAGE plpgsql
AS $BODY$

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
        Result := FALSE;
        RETURN;
    END IF;

    -- Check if the draw results exist
    SELECT EXISTS (
        SELECT 1 FROM Draw_Results dr
        JOIN Draws d ON dr.Draw_Id = d.Id
        WHERE d.Lottery_Id = LotteryId AND d.Draw_Date = DrawDate
        AND dr.Numbers = WinningNumbers
        AND (dr.Bonus_Numbers IS NULL OR dr.Bonus_Numbers = BonusNumbers)
    ) INTO DrawResultExists;

    Result := DrawResultExists;
END;
$$;
