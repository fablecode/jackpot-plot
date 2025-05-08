-- Step 1: Add 'lottery_id' column with a default value of 1
-- NOTE: This assumes that Lotteries has an entry with id = 1
ALTER TABLE tickets
ADD COLUMN lottery_id INT NOT NULL DEFAULT 1;

-- Step 2: Add foreign key constraint to reference Lotteries table
ALTER TABLE tickets
ADD CONSTRAINT fk_tickets_lottery
  FOREIGN KEY (lottery_id)
  REFERENCES Lotteries(id);

-- Step 3: Remove the default so future inserts must specify lottery_id explicitly
ALTER TABLE tickets
ALTER COLUMN lottery_id DROP DEFAULT;