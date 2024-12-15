CREATE TABLE Predictions (
    Id SERIAL PRIMARY KEY,
    UserId INT,                           -- Nullable for global predictions (not user-specific)
    LotteryId INT NOT NULL,               -- Foreign key to the Lotteries table
    PredictionNumbers INT[] NOT NULL,     -- Array of predicted numbers
    ConfidenceScore DECIMAL(5, 2),        -- Confidence score for the prediction
    GeneratedAt TIMESTAMP DEFAULT NOW(),  -- Timestamp for prediction generation
    UNIQUE (LotteryId, UserId, GeneratedAt) -- Prevent duplicate predictions for the same user and lottery
);
