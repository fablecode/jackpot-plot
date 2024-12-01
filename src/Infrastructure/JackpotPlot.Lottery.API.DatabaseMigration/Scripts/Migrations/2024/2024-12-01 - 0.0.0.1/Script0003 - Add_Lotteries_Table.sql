CREATE TABLE lotteries (
    id SERIAL PRIMARY KEY,             -- Unique identifier for each lottery
    name VARCHAR(255) NOT NULL,                 -- Name of the lottery (e.g., "Mega Millions")
    description TEXT,                           -- Description of the lottery
    draw_frequency VARCHAR(50),                 -- Frequency of the draw (e.g., "Weekly", "Daily")
    status VARCHAR(50) DEFAULT 'active',       -- Status of the lottery (e.g., 'active', 'inactive')
    created_at TIMESTAMPTZ DEFAULT NOW(),      -- Timestamp when the lottery was created
    updated_at TIMESTAMPTZ DEFAULT NOW()       -- Timestamp when the lottery information was last updated
);
