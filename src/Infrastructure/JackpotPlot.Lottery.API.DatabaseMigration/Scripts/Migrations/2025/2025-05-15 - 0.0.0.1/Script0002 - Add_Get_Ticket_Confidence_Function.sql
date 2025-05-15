CREATE OR REPLACE FUNCTION get_ticket_confidence(p_ticket_id UUID)
RETURNS TEXT AS $$
DECLARE
    v_lottery_id INT;
    play RECORD;
    draw RECORD;
    top_numbers INT[];
    line_numbers INT[];
    draw_numbers INT[];
    freq_score INT := 0;
    match_score INT := 0;
    total_lines INT := 0;
    total_score FLOAT;
    final_score FLOAT;
BEGIN
    -- Step 1: Get the lottery_id from the ticket
    SELECT t.lottery_id INTO v_lottery_id
    FROM tickets t
    WHERE t.id = p_ticket_id;

    IF v_lottery_id IS NULL THEN
        RETURN 'None';
    END IF;

    -- Step 2: Get top 10 frequent numbers from last 50 draws for this lottery
    SELECT ARRAY(
        SELECT n
        FROM (
            SELECT unnest(dr.numbers) AS n
            FROM draws d
            JOIN draw_results dr ON dr.draw_id = d.id
            WHERE d.lottery_id = v_lottery_id
            ORDER BY d.draw_date DESC
            LIMIT 50
        ) all_nums
        GROUP BY n
        ORDER BY COUNT(*) DESC
        LIMIT 10
    ) INTO top_numbers;

    -- Step 3: Loop through all plays on this ticket
    FOR play IN
        SELECT numbers FROM ticket_plays WHERE ticket_id = p_ticket_id
    LOOP
        total_lines := total_lines + 1;
        line_numbers := play.numbers;

        -- Score frequent number matches
        freq_score := freq_score + (
            SELECT COUNT(*) FROM unnest(line_numbers) n WHERE n = ANY(top_numbers)
        );

        -- Score historical match hits from recent draws
        FOR draw IN
            SELECT dr.numbers
            FROM draws d
            JOIN draw_results dr ON dr.draw_id = d.id
            WHERE d.lottery_id = v_lottery_id
            ORDER BY d.draw_date DESC
            LIMIT 20
        LOOP
            draw_numbers := draw.numbers;
            match_score := match_score + (
                SELECT COUNT(*) FROM unnest(line_numbers) n WHERE n = ANY(draw_numbers)
            );
        END LOOP;
    END LOOP;

    -- Step 4: Return classification
    IF total_lines = 0 THEN
        RETURN 'None';
    END IF;

    total_score := freq_score + match_score;
    final_score := total_score::FLOAT / (total_lines * (10 + 6 * 20)); -- 130 max per line

    IF final_score >= 0.8 THEN
        RETURN 'High';
    ELSIF final_score >= 0.5 THEN
        RETURN 'Medium';
    ELSIF final_score >= 0.2 THEN
        RETURN 'Low';
    ELSE
        RETURN 'None';
    END IF;
END;
$$ LANGUAGE plpgsql;