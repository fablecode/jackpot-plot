CREATE TABLE LotteryHistory (
    Id SERIAL PRIMARY KEY,
    LotteryId INT NOT NULL,               -- Foreign key to the Lotteries table (external or replicated)
    DrawDate TIMESTAMP NOT NULL,          -- Date of the draw
    WinningNumbers INT[] NOT NULL,        -- Array of winning numbers
    BonusNumbers INT[],                   -- Array of bonus numbers (nullable for lotteries without bonus)
    CreatedAt TIMESTAMP DEFAULT NOW(),    -- Timestamp for record creation
    UNIQUE (LotteryId, DrawDate)          -- Prevent duplicate entries for the same draw
);
