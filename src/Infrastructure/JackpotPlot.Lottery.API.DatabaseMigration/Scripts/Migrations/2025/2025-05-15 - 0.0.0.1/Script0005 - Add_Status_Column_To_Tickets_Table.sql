-- Create the enum type
CREATE TYPE ticket_status AS ENUM ('active', 'paused', 'excluded');

-- Add column with enum
ALTER TABLE tickets
ADD COLUMN status ticket_status;

-- Set existing values
UPDATE tickets
SET status = 'active';

-- Enforce NOT NULL and default
ALTER TABLE tickets
ALTER COLUMN status SET NOT NULL;
ALTER TABLE tickets
ALTER COLUMN status SET DEFAULT 'active';