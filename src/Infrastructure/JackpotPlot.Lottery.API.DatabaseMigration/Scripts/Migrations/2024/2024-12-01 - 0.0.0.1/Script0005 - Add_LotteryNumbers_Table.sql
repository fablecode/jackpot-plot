CREATE TABLE lottery_numbers (
    id SERIAL PRIMARY KEY,      -- Unique identifier for the drawn number set
    draw_id INT REFERENCES draws(id) ON DELETE CASCADE,  -- The draw this number set belongs to
    numbers INT[] NOT NULL,                    -- Array of drawn numbers (can be adjusted for different lottery formats)
    bonus_numbers INT[],                       -- Bonus numbers (if applicable, like Powerball)
    created_at TIMESTAMPTZ DEFAULT NOW(),       -- Timestamp when the lottery numbers were entered
    updated_at TIMESTAMPTZ DEFAULT NOW()        -- Timestamp when the lottery numbers were last updated
);
