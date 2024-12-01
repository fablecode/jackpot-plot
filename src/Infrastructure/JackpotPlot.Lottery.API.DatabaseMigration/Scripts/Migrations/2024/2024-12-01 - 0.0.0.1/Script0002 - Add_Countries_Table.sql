-- Create the table to store country information
CREATE TABLE countries (
    id SERIAL PRIMARY KEY,            -- Unique identifier for each country
    name VARCHAR(150) NOT NULL,       -- Name of the country
    continent_id INT NOT NULL,        -- Foreign key to the continents table
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL, -- Record creation timestamp
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL, -- Record last update timestamp
    CONSTRAINT fk_continent
        FOREIGN KEY (continent_id) REFERENCES continents (id) -- Foreign key constraint
        ON DELETE CASCADE                                         -- Cascade delete for related records
);
