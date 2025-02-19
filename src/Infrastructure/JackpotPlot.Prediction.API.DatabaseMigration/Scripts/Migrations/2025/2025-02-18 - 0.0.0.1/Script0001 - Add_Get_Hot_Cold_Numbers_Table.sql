CREATE OR REPLACE FUNCTION get_hot_cold_numbers(
    lottery_id INT, 
    time_range INTERVAL, 
    requested_numbers INT[], 
    number_type VARCHAR(10)
)
RETURNS TABLE (
    number INT,
    frequency INT,
    status VARCHAR(10)
) AS $$
BEGIN
    RETURN QUERY
    WITH NumberFrequency AS (
        SELECT 
            unnest(CASE WHEN number_type = 'main' THEN WinningNumbers ELSE BonusNumbers END) AS number, 
            COUNT(*)::INTEGER AS frequency
        FROM LotteryHistory
        WHERE LotteryId = lottery_id
        AND DrawDate >= NOW() - time_range
        GROUP BY number
    ),
    Thresholds AS (
        SELECT 
            percentile_cont(0.75) WITHIN GROUP (ORDER BY nf.frequency) AS hot_threshold, 
            percentile_cont(0.25) WITHIN GROUP (ORDER BY nf.frequency) AS cold_threshold
        FROM NumberFrequency nf
    ),
    ClassifiedNumbers AS (
        SELECT 
            nf.number, 
            nf.frequency, 
            CASE 
                WHEN nf.frequency >= t.hot_threshold THEN 'hot'::VARCHAR(10)  
                WHEN nf.frequency <= t.cold_threshold THEN 'cold'::VARCHAR(10)  
                ELSE 'neutral'::VARCHAR(10)  
            END AS status
        FROM NumberFrequency nf, Thresholds t
    )
    -- Ensure all requested numbers are returned
    SELECT 
        rn.number, 
        COALESCE(cn.frequency, 0) AS frequency,  
        COALESCE(cn.status, 'neutral') AS status
    FROM unnest(requested_numbers) AS rn(number)
    LEFT JOIN ClassifiedNumbers cn ON rn.number = cn.number
    ORDER BY rn.number;
END;
$$ LANGUAGE plpgsql;
