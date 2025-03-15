CREATE OR REPLACE FUNCTION get_frequent_prediction_pairs()
RETURNS TABLE(number1 INT, number2 INT, frequency BIGINT) AS $$
BEGIN
    RETURN QUERY
    WITH Flattened AS (
        SELECT p.id, unnest(p.predicted_numbers) AS number
        FROM predictions p
    ),
    PairCounts AS (
        SELECT 
            f1.number AS num1, 
            f2.number AS num2, 
            COUNT(*) AS freq
        FROM Flattened f1
        JOIN Flattened f2 ON f1.id = f2.id AND f1.number < f2.number -- ✅ Ensures unique pairs (e.g., 12-34 is the same as 34-12)
        GROUP BY f1.number, f2.number
    )
    SELECT PairCounts.num1 AS number1, PairCounts.num2 AS number2, PairCounts.freq AS frequency
    FROM PairCounts
    ORDER BY PairCounts.freq DESC
    LIMIT 20; -- ✅ Limit to the top 20 most frequent pairs
END;
$$ LANGUAGE plpgsql;