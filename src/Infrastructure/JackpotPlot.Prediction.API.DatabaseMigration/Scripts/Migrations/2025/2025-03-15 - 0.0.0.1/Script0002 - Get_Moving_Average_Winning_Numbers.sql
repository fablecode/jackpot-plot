CREATE OR REPLACE FUNCTION get_moving_average_winning_numbers(
    p_lottery_id INT,
    p_window_size INT
)
RETURNS TABLE(
    draw_date TIMESTAMP,
    winning_number INT,
    moving_average DECIMAL(10, 2)
)
AS $$
BEGIN
    RETURN QUERY
    WITH all_draws AS (
        SELECT DrawDate
        FROM LotteryHistory
        WHERE LotteryId = p_lottery_id
        ORDER BY DrawDate
    ),
    numbered_draws AS (
        SELECT
            lh.DrawDate,
            unnest(lh.WinningNumbers) AS winning_number
        FROM LotteryHistory lh
        WHERE lh.LotteryId = p_lottery_id
    ),
    draw_numbers AS (
        SELECT
            ad.DrawDate,
            wn.number AS winning_number
        FROM all_draws ad
        CROSS JOIN (
            SELECT DISTINCT winning_number AS number FROM numbered_draws
        ) wn
    ),
    occurrences AS (
        SELECT
            dn.DrawDate,
            dn.winning_number,
            COUNT(nd.winning_number) AS occurrence_count
        FROM draw_numbers dn
        LEFT JOIN numbered_draws nd
            ON dn.DrawDate = nd.DrawDate AND dn.winning_number = nd.winning_number
        GROUP BY dn.DrawDate, dn.winning_number
    ),
    moving_avg AS (
        SELECT
            o.DrawDate,
            o.winning_number,
            ROUND(
                AVG(o.occurrence_count::DECIMAL) OVER (
                    PARTITION BY o.winning_number
                    ORDER BY o.DrawDate
                    ROWS BETWEEN p_window_size - 1 PRECEDING AND CURRENT ROW
                ), 2
            ) AS moving_average
        FROM occurrences o
    )
    SELECT *
    FROM moving_avg
    ORDER BY DrawDate, winning_number;
END;
$$ LANGUAGE plpgsql;
