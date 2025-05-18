CREATE OR REPLACE VIEW ticket_overview AS
SELECT
    t.user_id,
    t.id AS ticket_id,
    t.name AS ticket_name,
    l.name AS lottery_name,
    t.status,
    COALESCE(tp.entry_count, 0) AS entries,
    t.confidence
FROM tickets t
JOIN lotteries l ON t.lottery_id = l.id
LEFT JOIN (
    SELECT ticket_id, COUNT(*) AS entry_count
    FROM ticket_plays
    GROUP BY ticket_id
) tp ON tp.ticket_id = t.id;