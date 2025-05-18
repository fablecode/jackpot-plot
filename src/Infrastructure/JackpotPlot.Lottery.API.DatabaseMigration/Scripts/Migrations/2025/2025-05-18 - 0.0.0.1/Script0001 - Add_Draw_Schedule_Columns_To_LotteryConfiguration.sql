-- Script0001 - Add_Draw_Schedule_Columns_To_LotteryConfiguration.sql

ALTER TABLE lottery_configuration
ADD COLUMN draw_frequency VARCHAR(50),           -- 'daily', 'weekly', or comma-separated days like 'mon,wed,fri'
ADD COLUMN interval_days INT,                    -- for interval draws (e.g. every 3 days)
ADD COLUMN draw_days TEXT[];                     -- PostgreSQL array of days ['Monday', 'Thursday']