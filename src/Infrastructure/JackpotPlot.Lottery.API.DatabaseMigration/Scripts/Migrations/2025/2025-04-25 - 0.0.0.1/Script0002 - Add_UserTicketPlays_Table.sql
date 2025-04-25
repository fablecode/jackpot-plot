CREATE TABLE user_ticket_plays (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id UUID NOT NULL REFERENCES user_tickets(id) ON DELETE CASCADE,
    numbers INT[] NOT NULL,         -- e.g. ARRAY[5, 11, 19, 27, 34, 44]
    line_index INT NOT NULL,             -- useful for ordering/position
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);