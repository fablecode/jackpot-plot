CREATE TYPE ticket_overview_row AS (
    ticket_id INT,
    ticket_name TEXT,
    lottery_name TEXT,
    status TEXT,
    next_draw DATE,
    entries INT,
    last_result TEXT,
    confidence TEXT
);