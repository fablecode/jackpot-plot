CREATE OR REPLACE FUNCTION get_lucky_pair_frequency()
RETURNS TABLE(number1 INT, number2 INT, frequency INT) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        wn1.number AS number1, 
        wn2.number AS number2,
        COUNT(*) AS frequency
    FROM lotteryhistory lh
    CROSS JOIN LATERAL unnest(lh.winningnumbers) AS wn1(number)
    CROSS JOIN LATERAL unnest(lh.winningnumbers) AS wn2(number)
    WHERE wn1.number < wn2.number -- ✅ Avoid duplicate pairs (e.g., 12-34 is the same as 34-12)
    GROUP BY wn1.number, wn2.number
    ORDER BY frequency DESC;
END;
$$ LANGUAGE plpgsql;
