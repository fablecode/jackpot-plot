CREATE TABLE lottery_configuration (
    id SERIAL PRIMARY KEY,
    lottery_id INT NOT NULL,                       -- Foreign key to lotteries table
    draw_type VARCHAR(50) DEFAULT 'regular',       -- Regular, jackpot, etc.
    main_numbers_count INT NOT NULL,               -- Number of main numbers in the draw
    main_numbers_range INT NOT NULL,               -- Range of main numbers (e.g., 1-50)
    bonus_numbers_count INT DEFAULT 0,             -- Number of bonus numbers (optional)
    bonus_numbers_range INT DEFAULT 0,             -- Range of bonus numbers (optional)
    start_date TIMESTAMP DEFAULT NOW(),            -- When this configuration starts
    end_date TIMESTAMP,                            -- When this configuration ends
    created_at TIMESTAMP DEFAULT NOW(),            -- Creation timestamp
    updated_at TIMESTAMP DEFAULT NOW(),            -- Last updated timestamp
    FOREIGN KEY (lottery_id) REFERENCES lotteries(id) ON DELETE CASCADE
);
