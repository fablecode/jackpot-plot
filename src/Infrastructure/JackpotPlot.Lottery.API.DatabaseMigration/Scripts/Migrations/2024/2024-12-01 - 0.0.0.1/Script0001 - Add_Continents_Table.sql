-- Create the table to store continent information
CREATE TABLE continents (
    id SERIAL PRIMARY KEY,          -- Unique identifier for each continent
    name VARCHAR(100) NOT NULL,     -- Name of the continent
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL, -- Record creation timestamp
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL  -- Record last update timestamp
);
