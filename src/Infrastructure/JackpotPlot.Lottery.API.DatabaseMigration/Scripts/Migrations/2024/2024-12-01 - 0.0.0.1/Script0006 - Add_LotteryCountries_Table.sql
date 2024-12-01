CREATE TABLE lotteries_countries (
    lottery_id INT REFERENCES lotteries(id) ON DELETE CASCADE,  -- Associated lottery
    country_id INT REFERENCES countries(id) ON DELETE CASCADE,  -- Associated country
    PRIMARY KEY (lottery_id, country_id),                               -- Composite primary key
    created_at TIMESTAMPTZ DEFAULT NOW(),                               -- Timestamp of creation
    updated_at TIMESTAMPTZ DEFAULT NOW()                                -- Timestamp of the last update
);
