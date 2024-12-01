CREATE TABLE draws (
    id SERIAL PRIMARY KEY,                -- Unique identifier for each draw
    lottery_id INT REFERENCES lotteries(id) ON DELETE CASCADE, -- The lottery game this draw belongs to
    draw_date DATE NOT NULL,                    -- Date of the draw
    draw_time TIME,                             -- Time of the draw (if applicable)
    jackpot_amount DECIMAL(15, 2),              -- Jackpot amount for this draw
    rollover_count INT DEFAULT 0,               -- Number of consecutive rollovers
    created_at TIMESTAMPTZ DEFAULT NOW(),       -- Timestamp of draw creation
    updated_at TIMESTAMPTZ DEFAULT NOW()        -- Timestamp when the draw information was last updated
);
