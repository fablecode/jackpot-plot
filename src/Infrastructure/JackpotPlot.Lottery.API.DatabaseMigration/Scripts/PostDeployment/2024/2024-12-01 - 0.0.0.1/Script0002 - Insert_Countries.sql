-- Insert records for countries grouped by continent, using variables for continent_ids

-- Africa
DO $$ 
DECLARE
    africa_id INT;
BEGIN
    -- Get the continent_id for Africa
    SELECT id INTO africa_id FROM continents WHERE name = 'Africa';
    
    -- Insert countries for Africa
    INSERT INTO countries (name, continent_id) VALUES
    ('Algeria', africa_id),
    ('Angola', africa_id),
    ('Benin', africa_id),
    ('Botswana', africa_id),
    ('Burkina Faso', africa_id),
    ('Burundi', africa_id),
    ('Cameroon', africa_id),
    ('Cape Verde', africa_id),
    ('Central African Republic', africa_id),
    ('Chad', africa_id),
    ('Comoros', africa_id),
    ('Congo (Congo-Brazzaville)', africa_id),
    ('Djibouti', africa_id),
    ('Egypt', africa_id),
    ('Equatorial Guinea', africa_id),
    ('Eritrea', africa_id),
    ('Eswatini (fmr. "Swaziland")', africa_id),
    ('Ethiopia', africa_id),
    ('Gabon', africa_id),
    ('Gambia', africa_id),
    ('Ghana', africa_id),
    ('Guinea', africa_id),
    ('Guinea-Bissau', africa_id),
    ('Ivory Coast', africa_id),
    ('Kenya', africa_id),
    ('Lesotho', africa_id),
    ('Liberia', africa_id),
    ('Libya', africa_id),
    ('Madagascar', africa_id),
    ('Malawi', africa_id),
    ('Mali', africa_id),
    ('Mauritania', africa_id),
    ('Mauritius', africa_id),
    ('Morocco', africa_id),
    ('Mozambique', africa_id),
    ('Namibia', africa_id),
    ('Niger', africa_id),
    ('Nigeria', africa_id),
    ('Rwanda', africa_id),
    ('Sao Tome and Principe', africa_id),
    ('Senegal', africa_id),
    ('Seychelles', africa_id),
    ('Sierra Leone', africa_id),
    ('Somalia', africa_id),
    ('South Africa', africa_id),
    ('South Sudan', africa_id),
    ('Sudan', africa_id),
    ('Tanzania', africa_id),
    ('Togo', africa_id),
    ('Tunisia', africa_id),
    ('Uganda', africa_id),
    ('Zambia', africa_id),
    ('Zimbabwe', africa_id);
END $$;

-- Antarctica
DO $$ 
DECLARE
    antarctica_id INT;
BEGIN
    -- Get the continent_id for Antarctica
    SELECT id INTO antarctica_id FROM continents WHERE name = 'Antarctica';
    
    -- Insert countries for Antarctica
    INSERT INTO countries (name, continent_id) VALUES
    ('Antarctica', antarctica_id);
END $$;

-- Asia
DO $$ 
DECLARE
    asia_id INT;
BEGIN
    -- Get the continent_id for Asia
    SELECT id INTO asia_id FROM continents WHERE name = 'Asia';
    
    -- Insert countries for Asia
    INSERT INTO countries (name, continent_id) VALUES
    ('Afghanistan', asia_id),
    ('Armenia', asia_id),
    ('Azerbaijan', asia_id),
    ('Bahrain', asia_id),
    ('Bangladesh', asia_id),
    ('Bhutan', asia_id),
    ('Brunei', asia_id),
    ('Cambodia', asia_id),
    ('China', asia_id),
    ('Cyprus', asia_id),
    ('Georgia', asia_id),
    ('India', asia_id),
    ('Indonesia', asia_id),
    ('Iran', asia_id),
    ('Iraq', asia_id),
    ('Israel', asia_id),
    ('Japan', asia_id),
    ('Jordan', asia_id),
    ('Kazakhstan', asia_id),
    ('Kuwait', asia_id),
    ('Kyrgyzstan', asia_id),
    ('Laos', asia_id),
    ('Lebanon', asia_id),
    ('Malaysia', asia_id),
    ('Maldives', asia_id),
    ('Mongolia', asia_id),
    ('Myanmar (Burma)', asia_id),
    ('Nepal', asia_id),
    ('North Korea', asia_id),
    ('Oman', asia_id),
    ('Pakistan', asia_id),
    ('Palestine State', asia_id),
    ('Philippines', asia_id),
    ('Qatar', asia_id),
    ('Saudi Arabia', asia_id),
    ('Singapore', asia_id),
    ('South Korea', asia_id),
    ('Sri Lanka', asia_id),
    ('Syria', asia_id),
    ('Tajikistan', asia_id),
    ('Thailand', asia_id),
    ('Timor-Leste', asia_id),
    ('Turkey', asia_id),
    ('Turkmenistan', asia_id),
    ('United Arab Emirates', asia_id),
    ('Uzbekistan', asia_id),
    ('Vietnam', asia_id),
    ('Yemen', asia_id);
END $$;

-- Europe
DO $$ 
DECLARE
    europe_id INT;
BEGIN
    -- Get the continent_id for Europe
    SELECT id INTO europe_id FROM continents WHERE name = 'Europe';
    
    -- Insert countries for Europe
    INSERT INTO countries (name, continent_id) VALUES
    ('Albania', europe_id),
    ('Andorra', europe_id),
    ('Austria', europe_id),
    ('Belarus', europe_id),
    ('Belgium', europe_id),
    ('Bosnia and Herzegovina', europe_id),
    ('Bulgaria', europe_id),
    ('Croatia', europe_id),
    ('Cyprus', europe_id),
    ('Czech Republic', europe_id),
    ('Denmark', europe_id),
    ('Estonia', europe_id),
    ('Finland', europe_id),
    ('France', europe_id),
    ('Germany', europe_id),
    ('Greece', europe_id),
    ('Hungary', europe_id),
    ('Iceland', europe_id),
    ('Ireland', europe_id),
    ('Italy', europe_id),
    ('Kosovo', europe_id),
    ('Latvia', europe_id),
    ('Liechtenstein', europe_id),
    ('Lithuania', europe_id),
    ('Luxembourg', europe_id),
    ('Malta', europe_id),
    ('Moldova', europe_id),
    ('Monaco', europe_id),
    ('Montenegro', europe_id),
    ('Netherlands', europe_id),
    ('North Macedonia', europe_id),
    ('Norway', europe_id),
    ('Poland', europe_id),
    ('Portugal', europe_id),
    ('Romania', europe_id),
    ('Russia', europe_id),
    ('San Marino', europe_id),
    ('Serbia', europe_id),
    ('Slovakia', europe_id),
    ('Slovenia', europe_id),
    ('Spain', europe_id),
    ('Sweden', europe_id),
    ('Switzerland', europe_id),
    ('Ukraine', europe_id),
    ('United Kingdom', europe_id),
    ('Vatican City', europe_id);
END $$;

-- North America
DO $$ 
DECLARE
    north_america_id INT;
BEGIN
    -- Get the continent_id for North America
    SELECT id INTO north_america_id FROM continents WHERE name = 'North America';
    
    -- Insert countries for North America
    INSERT INTO countries (name, continent_id) VALUES
    ('Antigua and Barbuda', north_america_id),
    ('Bahamas', north_america_id),
    ('Barbados', north_america_id),
    ('Belize', north_america_id),
    ('Canada', north_america_id),
    ('Costa Rica', north_america_id),
    ('Cuba', north_america_id),
    ('Dominica', north_america_id),
    ('Dominican Republic', north_america_id),
    ('El Salvador', north_america_id),
    ('Grenada', north_america_id),
    ('Guatemala', north_america_id),
    ('Haiti', north_america_id),
    ('Honduras', north_america_id),
    ('Jamaica', north_america_id),
    ('Mexico', north_america_id),
    ('Nicaragua', north_america_id),
    ('Panama', north_america_id),
    ('Saint Kitts and Nevis', north_america_id),
    ('Saint Lucia', north_america_id),
    ('Saint Vincent and the Grenadines', north_america_id),
    ('Trinidad and Tobago', north_america_id),
    ('United States of America', north_america_id);
END $$;

-- Australia (Oceania)
DO $$ 
DECLARE
    australia_id INT;
BEGIN
    -- Get the continent_id for Australia (Oceania)
    SELECT id INTO australia_id FROM continents WHERE name = 'Australia';
    
    -- Insert countries for Australia (Oceania)
    INSERT INTO countries (name, continent_id) VALUES
    ('Australia', australia_id),
    ('Fiji', australia_id),
    ('Kiribati', australia_id),
    ('Marshall Islands', australia_id),
    ('Micronesia', australia_id),
    ('Nauru', australia_id),
    ('New Zealand', australia_id),
    ('Palau', australia_id),
    ('Papua New Guinea', australia_id),
    ('Samoa', australia_id),
    ('Solomon Islands', australia_id),
    ('Tonga', australia_id),
    ('Tuvalu', australia_id),
    ('Vanuatu', australia_id);
END $$;

-- South America
DO $$ 
DECLARE
    south_america_id INT;
BEGIN
    -- Get the continent_id for South America
    SELECT id INTO south_america_id FROM continents WHERE name = 'South America';
    
    -- Insert countries for South America
    INSERT INTO countries (name, continent_id) VALUES
    ('Argentina', south_america_id),
    ('Bolivia', south_america_id),
    ('Brazil', south_america_id),
    ('Chile', south_america_id),
    ('Colombia', south_america_id),
    ('Ecuador', south_america_id),
    ('Guyana', south_america_id),
    ('Paraguay', south_america_id),
    ('Peru', south_america_id),
    ('Suriname', south_america_id),
    ('Uruguay', south_america_id),
    ('Venezuela', south_america_id);
END $$;
