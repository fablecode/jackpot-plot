CREATE TABLE predictions (
    id SERIAL PRIMARY KEY,       -- Unique identifier for the prediction
    lottery_id INT NOT NULL,                -- Foreign key to the lotteries table
    user_id INT,                            -- Nullable: ID of the user (if applicable)
    strategy VARCHAR(50) NOT NULL,          -- The prediction strategy used (e.g., "frequency-based")
    predicted_numbers INT[] NOT NULL,       -- Array of main predicted numbers
    bonus_numbers INT[],                    -- Array of bonus predicted numbers (nullable)
    confidence_score DECIMAL(5, 2),         -- Confidence score (e.g., 0.85 for 85%)
    created_at TIMESTAMP DEFAULT NOW()      -- Timestamp for when the prediction was made
);
