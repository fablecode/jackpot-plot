-- 0. Drop the existing CHECK constraint if it exists
ALTER TABLE tickets
DROP CONSTRAINT IF EXISTS chk_confidence_valid;

-- 1. Create the ENUM type if it doesn't exist yet
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'confidence_level') THEN
        CREATE TYPE confidence_level AS ENUM ('high', 'medium', 'low', 'none');
    END IF;
END$$;

-- 2. Normalize existing values to lowercase
UPDATE tickets
SET confidence = LOWER(confidence)
WHERE confidence IS NOT NULL;

-- 3. Alter the column type to use the enum (cast from lowercase strings)
ALTER TABLE tickets
ALTER COLUMN confidence TYPE confidence_level
USING confidence::confidence_level;

-- 4. Ensure the column is NOT NULL
ALTER TABLE tickets
ALTER COLUMN confidence SET NOT NULL;

-- 5. Set default value for future inserts
ALTER TABLE tickets
ALTER COLUMN confidence SET DEFAULT 'none';
