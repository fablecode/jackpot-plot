-- 1. Add the column allowing NULLs first
ALTER TABLE Tickets
ADD COLUMN confidence TEXT;

-- 2. Populate existing rows with default value
UPDATE Tickets
SET confidence = 'None'
WHERE confidence IS NULL;

-- 3. Add the CHECK constraint
ALTER TABLE Tickets
ADD CONSTRAINT chk_confidence_valid
CHECK (confidence IN ('High', 'Medium', 'Low', 'None'));

-- 4. Enforce NOT NULL
ALTER TABLE Tickets
ALTER COLUMN confidence SET NOT NULL;
