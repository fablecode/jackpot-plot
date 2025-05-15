-- 1. Drop the existing type that includes next_draw and last_result
DROP TYPE IF EXISTS ticket_overview_row;

-- 2. Recreate the type without next_draw and last_result
CREATE TYPE ticket_overview_row AS (
    ticket_id INT,
    ticket_name TEXT,
    lottery_name TEXT,
    status TEXT,
    entries INT,
    confidence TEXT
);