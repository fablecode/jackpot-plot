CREATE OR REPLACE FUNCTION get_winning_number_frequency_over_time()
RETURNS TABLE(number INTEGER, drawdate DATE, frequency BIGINT) AS $$
BEGIN
    RETURN QUERY
	SELECT 
        wn.number, 
        lh.drawdate::DATE, 
        COUNT(*) AS frequency
    FROM lotteryHistory lh
    CROSS JOIN LATERAL unnest(lh.winningnumbers) AS wn(number) -- ✅ Flatten array
    GROUP BY wn.number, lh.drawdate::DATE -- ✅ Aggregate occurrences per draw date
    ORDER BY lh.drawdate::DATE;
END;
$$ LANGUAGE plpgsql;