-- 1. Add a new temporary UUID column
ALTER TABLE predictions
ADD COLUMN user_id_uuid UUID;

-- 2. If you have a way to convert existing INT user_ids to UUIDs, do that here.
-- Example assumes you have a mapping table `user_id_map(int_id, uuid_id)`
-- UPDATE predictions
-- SET user_id_uuid = m.uuid_id
-- FROM user_id_map m
-- WHERE predictions.user_id = m.int_id;

-- If you're not preserving old data, you can skip the above and just leave the UUIDs null.

-- 3. Drop the old INT column
ALTER TABLE predictions
DROP COLUMN user_id;

-- 4. Rename the new UUID column to user_id
ALTER TABLE predictions
RENAME COLUMN user_id_uuid TO user_id;

-- 5. (Optional) Add a foreign key constraint if users table uses UUIDs
-- ALTER TABLE predictions
-- ADD CONSTRAINT fk_user
-- FOREIGN KEY (user_id)
-- REFERENCES users(id);
